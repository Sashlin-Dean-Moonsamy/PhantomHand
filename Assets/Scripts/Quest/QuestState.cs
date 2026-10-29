namespace QuestSystem
{
    /// <summary>
    /// Runtime, per-playthrough progress for a single quest. Not a ScriptableObject -
    /// this data changes constantly and is unique to the current save file.
    /// </summary>
    public class QuestState
    {
        public QuestSO quest;
        public int currentStepIndex;
        public int currentProgress;
        public QuestStatus status;

        public QuestState(QuestSO quest)
        {
            this.quest = quest;
            currentStepIndex = 0;
            currentProgress = 0;
            status = QuestStatus.NotStarted;
        }

        public QuestStepSO CurrentStep =>
            (quest != null && currentStepIndex < quest.steps.Count) ? quest.steps[currentStepIndex] : null;
    }
}
