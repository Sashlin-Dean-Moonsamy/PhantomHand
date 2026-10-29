using UnityEngine;
using UnityEngine.UI;

namespace QuestSystem
{
    /// <summary>
    /// Attach to a single row/entry prefab (e.g. QuestLogEntry.prefab) that gets
    /// instantiated once per quest inside the QuestLogUI panel.
    /// </summary>
    public class QuestLogEntryUI : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text stepText;
        [SerializeField] private Text progressText;

        private string questId;

        public void Bind(QuestState state)
        {
            questId = state.quest.questId;
            titleText.text = state.quest.title;

            var step = state.CurrentStep;
            if (state.status == QuestStatus.Completed)
            {
                stepText.text = "Completed";
                progressText.text = "";
            }
            else if (step != null)
            {
                stepText.text = step.instructionText;
                progressText.text = step.requiredAmount > 1
                    ? $"{state.currentProgress}/{step.requiredAmount}"
                    : "";
            }
        }

        public string QuestId => questId;
    }
}
