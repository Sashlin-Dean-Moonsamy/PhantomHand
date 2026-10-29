using TMPro;
using UnityEngine;

namespace QuestSystem
{
    /// <summary>
    /// Floating on-screen prompt box that shows the current tutorial step's instruction text
    /// (e.g. "Press Space to Jump"). Auto-shows when a tutorial step becomes active, and
    /// auto-hides when that tutorial completes or fails.
    ///
    /// Only reacts to quests flagged isTutorial = true, so normal quests never trigger this popup.
    /// </summary>
    public class TutorialPromptUI : MonoBehaviour
    {
        [Tooltip("The parent object (panel/background) to show and hide. Usually the same object this script is on, or its parent.")]
        [SerializeField] private GameObject root;

        [SerializeField] private TMP_Text instructionText;

        [Tooltip("Optional: a progress label, e.g. '2/3'. Leave empty to ignore.")]
        [SerializeField] private TMP_Text progressText;

        void OnEnable()
        {
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.OnQuestStarted += HandleQuestStarted;
            QuestManager.Instance.OnStepAdvanced += HandleStepAdvanced;
            QuestManager.Instance.OnQuestCompleted += HandleQuestEnded;
            QuestManager.Instance.OnQuestFailed += HandleQuestEnded;

            if (root != null) root.SetActive(false);

            // Catch up on any tutorial that started BEFORE this UI existed - e.g. if
            // QuestManager/TutorialManager live in a Bootloader scene and auto-start the
            // tutorial before the gameplay scene (with this UI) has even loaded.
            RefreshFromCurrentState();
        }

        /// <summary>Scans for any currently-active tutorial quest and displays its current step, if any.</summary>
        void RefreshFromCurrentState()
        {
            foreach (var state in QuestManager.Instance.GetAllStates())
            {
                if (state.quest.isTutorial && state.status == QuestStatus.Active)
                {
                    ShowStep(state);
                    return;
                }
            }
        }

        void OnDisable()
        {
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.OnQuestStarted -= HandleQuestStarted;
            QuestManager.Instance.OnStepAdvanced -= HandleStepAdvanced;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestEnded;
            QuestManager.Instance.OnQuestFailed -= HandleQuestEnded;
        }

        void HandleQuestStarted(QuestSO quest)
        {
            if (!quest.isTutorial) return;
            var state = QuestManager.Instance.GetState(quest.questId);
            ShowStep(state);
        }

        void HandleStepAdvanced(QuestState state)
        {
            if (!state.quest.isTutorial) return;
            ShowStep(state);
        }

        void HandleQuestEnded(QuestSO quest)
        {
            if (!quest.isTutorial) return;
            if (root != null) root.SetActive(false);
        }

        void ShowStep(QuestState state)
        {
            var step = state?.CurrentStep;
            if (step == null)
            {
                if (root != null) root.SetActive(false);
                return;
            }

            if (root != null) root.SetActive(true);
            if (instructionText != null) instructionText.text = step.instructionText;

            if (progressText != null)
            {
                progressText.text = step.requiredAmount > 1
                    ? $"{state.currentProgress}/{step.requiredAmount}"
                    : "";
            }
        }
    }
}
