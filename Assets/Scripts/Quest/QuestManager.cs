using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace QuestSystem
{
    /// <summary>
    /// Core brain of the quest system. Tracks active/completed quests, listens to GameEvents,
    /// advances steps, and fires events for UI to react to.
    ///
    /// Put this on a persistent GameObject (e.g. "QuestManager") in your first-loaded scene,
    /// and mark it DontDestroyOnLoad, OR use a persistent bootstrap scene pattern.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Tooltip("Every quest/tutorial in the game. Drag all your QuestSO assets in here.")]
        public List<QuestSO> allQuests = new List<QuestSO>();

        [Header("Tutorial Prompt UI")]
        [Tooltip("The panel that holds the instruction text (e.g. TutorialPromptPanel).")]
        public GameObject tutorialPromptPanel;

        [Tooltip("The child Text (TMP) component that displays the current quest's title.")]
        public TextMeshProUGUI titleText;

        [Tooltip("The child Text (TMP) component that displays the current step's instruction.")]
        public TextMeshProUGUI instructionText;

        private readonly Dictionary<string, QuestState> quests = new Dictionary<string, QuestState>();

        public event Action<QuestSO> OnQuestStarted;
        public event Action<QuestState> OnStepAdvanced;
        public event Action<QuestSO> OnQuestCompleted;
        public event Action<QuestSO> OnQuestFailed;


        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // DontDestroyOnLoad only works on a ROOT object. If this GameObject is nested
            // under something else in the hierarchy, use its root instead.
            DontDestroyOnLoad(transform.root.gameObject);

            foreach (var quest in allQuests)
            {
                if (quest != null && !quests.ContainsKey(quest.questId))
                    {
                    quests[quest.questId] = new QuestState(quest);
                    Debug.Log($"[QuestManager] Registered quest '{quest.questId}' ({quest.name}) with {quest.steps.Count} steps");
                    }
                else if (quest == null)
                {
                    Debug.LogWarning("[QuestManager] Null entry in allQuests list — check the Inspector.");
      
                }
            }
            
              
            Debug.Log($"[QuestManager] Total quests registered: {quests.Count}");

        }

        void Start()
            {
                foreach (var quest in allQuests)
                {
                    if (quest != null && quest.autoStart)
                    {
                        Debug.Log($"[QuestManager] Auto-starting quest '{quest.questId}'");
                        StartQuest(quest.questId);
                    }
                }
            }

        void OnEnable()
        {
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            GameEvents.OnItemCollected += HandleItemCollected;
            GameEvents.OnLocationReached += HandleLocationReached;
            GameEvents.OnNpcTalkedTo += HandleNpcTalkedTo;
            GameEvents.OnItemDelivered += HandleItemDelivered;
            GameEvents.OnItemUsed += HandleItemUsed;
        }

        void OnDisable()
        {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            GameEvents.OnItemCollected -= HandleItemCollected;
            GameEvents.OnLocationReached -= HandleLocationReached;
            GameEvents.OnNpcTalkedTo -= HandleNpcTalkedTo;
            GameEvents.OnItemDelivered -= HandleItemDelivered;
            GameEvents.OnItemUsed -= HandleItemUsed;
        }

        // --- Event handlers translate gameplay events into generic progress reports ---
        void HandleEnemyKilled(string id) => ReportProgress(QuestConditionType.Kill, id);
        void HandleItemCollected(string id, int amount) => ReportProgress(QuestConditionType.Collect, id, amount);
        void HandleLocationReached(string id) => ReportProgress(QuestConditionType.ReachLocation, id);
        void HandleNpcTalkedTo(string id) => ReportProgress(QuestConditionType.TalkTo, id);
        void HandleItemDelivered(string id) => ReportProgress(QuestConditionType.DeliverItem, id);
        void HandleItemUsed(string id) => ReportProgress(QuestConditionType.UseItem, id);

        // --- Public API ---

        public bool StartQuest(string questId)
        {
            if (!quests.TryGetValue(questId, out var state))
            {
                Debug.LogWarning($"[QuestManager] Tried to start unknown quest '{questId}'.");
                return false;
            }

            if (state.status == QuestStatus.Active || state.status == QuestStatus.Completed)
                return false;

            if (!state.quest.PrerequisitesMet(this))
            {
                Debug.LogWarning($"[QuestManager] Prerequisites not met for quest '{questId}'.");
                return false;
            }

            state.status = QuestStatus.Active;
            state.currentStepIndex = 0;
            state.currentProgress = 0;
            Debug.Log($"[QuestManager] Started quest: {questId}");
            OnQuestStarted?.Invoke(state.quest);
            UpdateInstructionUI(state);
            return true;
        }

        public void FailQuest(string questId)
        {
            if (quests.TryGetValue(questId, out var state) && state.status == QuestStatus.Active)
            {
                state.status = QuestStatus.Failed;
                OnQuestFailed?.Invoke(state.quest);
            }
        }

        public bool IsQuestCompleted(string questId) =>
            quests.TryGetValue(questId, out var state) && state.status == QuestStatus.Completed;

        public bool IsQuestActive(string questId) =>
            quests.TryGetValue(questId, out var state) && state.status == QuestStatus.Active;

        public QuestState GetState(string questId) =>
            quests.TryGetValue(questId, out var state) ? state : null;

        public IEnumerable<QuestState> GetAllStates() => quests.Values;

        /// <summary>
        /// Call this whenever a gameplay event happens that might satisfy a quest step.
        /// Also used directly by TutorialManager for input-based conditions.
        /// </summary>
        public void ReportProgress(QuestConditionType type, string targetId, int amount = 1)
        {
            foreach (var state in quests.Values)
            {
                if (state.status != QuestStatus.Active) continue;

                var step = state.CurrentStep;
                if (step == null) continue;

                if (step.conditionType == type && step.targetId == targetId)
                {
                    state.currentProgress += amount;
                    if (state.currentProgress >= step.requiredAmount)
                        AdvanceStep(state);
                }
            }
        }

        void AdvanceStep(QuestState state)
        {
            state.currentStepIndex++;
            state.currentProgress = 0;

            if (state.currentStepIndex >= state.quest.steps.Count)
            {
                state.status = QuestStatus.Completed;
                OnQuestCompleted?.Invoke(state.quest);
                GrantRewards(state.quest);

                // Hide the prompt panel once the quest is done (optional — remove if you
                // want it to stay visible or show a "complete" message instead).
                if (tutorialPromptPanel != null)
                    tutorialPromptPanel.SetActive(false);
            }
            else
            {
                OnStepAdvanced?.Invoke(state);
                UpdateInstructionUI(state);
            }
        }

        /// <summary>
        /// Pushes the current step's instruction text into the TutorialPromptPanel's
        /// InstructionText child, and makes sure the panel is visible.
        /// </summary>
        void UpdateInstructionUI(QuestState state)
        {
            var step = state.CurrentStep;
            if (step == null) return;

            if (tutorialPromptPanel != null)
                tutorialPromptPanel.SetActive(true);

            if (titleText != null)
                titleText.text = state.quest.title;

            if (instructionText != null)
                instructionText.text = step.instructionText;
        }

        void GrantRewards(QuestSO quest)
        {
            foreach (var reward in quest.rewards)
            {
                switch (reward.rewardType)
                {
                    case RewardType.Item:
                        // TODO: hook into your inventory system, e.g.
                        // InventoryManager.Instance.AddItem(reward.itemId, reward.amount);
                        Debug.Log($"[QuestManager] Granting item '{reward.itemId}' x{reward.amount}");
                        break;
                    case RewardType.Experience:
                        // TODO: hook into your XP/leveling system
                        Debug.Log($"[QuestManager] Granting {reward.amount} XP");
                        break;
                    case RewardType.Currency:
                        // TODO: hook into your currency/economy system
                        Debug.Log($"[QuestManager] Granting {reward.amount} currency");
                        break;
                }
            }
        }
    }
}