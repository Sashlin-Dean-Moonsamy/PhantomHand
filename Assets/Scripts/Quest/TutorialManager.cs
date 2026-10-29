using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QuestSystem
{
    /// <summary>
    /// Thin wrapper around QuestManager for tutorial-specific input events, built on the
    /// NEW Unity Input System (the "InputSystem_Actions" asset your project auto-creates).
    ///
    /// Instead of hardcoding KeyCodes, this drags in InputActionReferences that point at
    /// actions inside your InputSystem_Actions asset (e.g. Player/Jump, Player/Move).
    /// Assign these in the Inspector - no code changes needed when you add new tutorial steps.
    ///
    /// Tutorials are just QuestSOs with isTutorial = true and steps using the
    /// PressKey / ClickUI / OpenMenu / MoveInDirection / WaitForSeconds condition types.
    /// </summary>
    [RequireComponent(typeof(QuestManager))]
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [System.Serializable]
        public class ButtonActionBinding
        {
            [Tooltip("Drag an action from your InputSystem_Actions asset, e.g. Player/Jump.")]
            public InputActionReference action;

            [Tooltip("Must match the targetId on a QuestStepSO with conditionType PressKey.")]
            public string actionId;
        }

        [Header("Button-style actions (Jump, Interact, etc.)")]
        [Tooltip("Each entry hooks one InputAction's 'performed' event to a tutorial step target id.")]
        public List<ButtonActionBinding> buttonBindings = new List<ButtonActionBinding>();

        [Header("Movement action (for MoveInDirection steps)")]
        [Tooltip("Drag the Player/Move action here (a Vector2 action). Leave empty if you don't have movement tutorial steps.")]
        public InputActionReference moveAction;

        [Tooltip("How far the stick/keys must be pushed before a direction counts as 'moved'.")]
        [Range(0.1f, 1f)] public float moveThreshold = 0.5f;

        private const string SeenPrefsPrefix = "tutorial_seen_";
        private bool hasReportedLeft, hasReportedRight, hasReportedUp, hasReportedDown;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            AutoStartFirstPlayTutorials();
        }

        void OnEnable()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnStepAdvanced += HandleStepAdvanced;

            foreach (var binding in buttonBindings)
            {
                if (binding.action == null) continue;
                binding.action.action.Enable();
                binding.action.action.performed += ctx => HandleButtonAction(binding.actionId);
            }

            if (moveAction != null)
                moveAction.action.Enable();
        }

        void OnDisable()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnStepAdvanced -= HandleStepAdvanced;

            foreach (var binding in buttonBindings)
            {
                if (binding.action == null) continue;
                binding.action.action.Disable();
            }

            if (moveAction != null)
                moveAction.action.Disable();
        }

        void Update()
        {
            if (moveAction == null) return;

            Vector2 move = moveAction.action.ReadValue<Vector2>();

            // Report each direction once per "push" rather than every single frame.
            CheckDirection(move.x <= -moveThreshold, ref hasReportedLeft, "left");
            CheckDirection(move.x >= moveThreshold, ref hasReportedRight, "right");
            CheckDirection(move.y >= moveThreshold, ref hasReportedUp, "up");
            CheckDirection(move.y <= -moveThreshold, ref hasReportedDown, "down");
        }

        void CheckDirection(bool isPushed, ref bool alreadyReported, string directionId)
        {
            if (isPushed && !alreadyReported)
            {
                alreadyReported = true;
                QuestManager.Instance.ReportProgress(QuestConditionType.MoveInDirection, directionId);
            }
            else if (!isPushed && alreadyReported)
            {
                alreadyReported = false; // reset so the player can trigger it again if needed elsewhere
            }
        }

        void HandleButtonAction(string actionId)
        {
            QuestManager.Instance.ReportProgress(QuestConditionType.PressKey, actionId);
        }

        void AutoStartFirstPlayTutorials()
        {
            Debug.Log($"[TutorialManager] Checking {QuestManager.Instance.allQuests.Count} quests for auto-start tutorials...");

            foreach (var state in QuestManager.Instance.GetAllStates())
            {
                var quest = state.quest;
                Debug.Log($"[TutorialManager] Found quest '{quest.questId}' - isTutorial:{quest.isTutorial}, autoStart:{quest.autoStartOnFirstPlay}");

                if (!quest.isTutorial || !quest.autoStartOnFirstPlay) continue;

                string prefsKey = SeenPrefsPrefix + quest.questId;
                bool alreadySeen = PlayerPrefs.GetInt(prefsKey, 0) == 1;
                Debug.Log($"[TutorialManager] '{quest.questId}' alreadySeen (PlayerPrefs): {alreadySeen}");
                if (alreadySeen) continue; // already seen

                bool started = QuestManager.Instance.StartQuest(quest.questId);
                Debug.Log($"[TutorialManager] StartQuest('{quest.questId}') returned: {started}");

                PlayerPrefs.SetInt(prefsKey, 1);
                PlayerPrefs.Save();

                ShowHighlightForCurrentStep(quest.questId);
                break; // usually only run one tutorial at a time
            }
        }

        void HandleStepAdvanced(QuestState state)
        {
            if (!state.quest.isTutorial) return;
            ShowHighlightForCurrentStep(state.quest.questId);
        }

        void ShowHighlightForCurrentStep(string questId)
        {
            var state = QuestManager.Instance.GetState(questId);
            var step = state?.CurrentStep;
            if (step != null && step.highlightTarget)
                TutorialHighlighter.Instance?.HighlightTarget(step.targetId);
            else
                TutorialHighlighter.Instance?.ClearHighlight();
        }

        // --- Call these from UI Button OnClick events, menu-open logic, etc. ---
        public void ReportUiClick(string uiId) => QuestManager.Instance.ReportProgress(QuestConditionType.ClickUI, uiId);
        public void ReportMenuOpened(string menuId) => QuestManager.Instance.ReportProgress(QuestConditionType.OpenMenu, menuId);

        /// <summary>Resets a tutorial's "seen" flag so it will play again (handy for a Settings > Replay Tutorial button).</summary>
        public void ResetTutorialSeenFlag(string questId)
        {
            PlayerPrefs.DeleteKey(SeenPrefsPrefix + questId);
        }
    }
}