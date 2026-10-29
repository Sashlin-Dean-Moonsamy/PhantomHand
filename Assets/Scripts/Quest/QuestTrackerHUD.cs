using UnityEngine;
using UnityEngine.UI;

namespace QuestSystem
{
    /// <summary>
    /// Small always-visible HUD element that shows the current step of one
    /// "tracked" quest (usually the most recently started or a player-pinned one).
    /// </summary>
    public class QuestTrackerHUD : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text questTitleText;
        [SerializeField] private Text stepText;
        [SerializeField] private Text progressText;

        private string trackedQuestId;

        void OnEnable()
        {
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
            QuestManager.Instance.OnStepAdvanced += HandleStepAdvanced;
            QuestManager.Instance.OnQuestCompleted += HandleQuestCompleted;

            root.SetActive(false);
        }

        void OnDisable()
        {
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
            QuestManager.Instance.OnStepAdvanced -= HandleStepAdvanced;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestCompleted;
        }

        /// <summary>Call this to pin a specific quest to the tracker (e.g. from the quest log UI).</summary>
        public void TrackQuest(string questId)
        {
            trackedQuestId = questId;
            Refresh();
        }

        void HandleQuestStarted(QuestSO quest)
        {
            // Auto-track the most recently started quest if nothing is tracked yet.
            if (string.IsNullOrEmpty(trackedQuestId))
                trackedQuestId = quest.questId;
            Refresh();
        }

        void HandleStepAdvanced(QuestState state)
        {
            if (state.quest.questId == trackedQuestId)
                Refresh();
        }

        void HandleQuestCompleted(QuestSO quest)
        {
            if (quest.questId == trackedQuestId)
            {
                trackedQuestId = null;
                root.SetActive(false);
            }
        }

        void Refresh()
        {
            if (string.IsNullOrEmpty(trackedQuestId))
            {
                root.SetActive(false);
                return;
            }

            var state = QuestManager.Instance.GetState(trackedQuestId);
            var step = state?.CurrentStep;
            if (state == null || step == null)
            {
                root.SetActive(false);
                return;
            }

            root.SetActive(true);
            questTitleText.text = state.quest.title;
            stepText.text = step.instructionText;
            progressText.text = step.requiredAmount > 1
                ? $"{state.currentProgress}/{step.requiredAmount}"
                : "";
        }
    }
}
