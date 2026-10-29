using UnityEngine;

namespace QuestSystem
{
    /// <summary>
    /// A single step within a quest (or tutorial). A QuestSO holds an ordered list of these.
    /// Create instances via Assets > Create > Quests > Step.
    /// </summary>
    [CreateAssetMenu(menuName = "Quests/Step", fileName = "Step_NewStep")]
    public class QuestStepSO : ScriptableObject
    {
        [Tooltip("Unique id for this step, used for save/load and debugging.")]
        public string stepId;

        [Tooltip("Text shown to the player in the quest tracker / tutorial prompt.")]
        [TextArea]
        public string instructionText;

        [Tooltip("What kind of event advances this step.")]
        public QuestConditionType conditionType;

        [Tooltip("The id of the thing this step cares about, e.g. 'goblin', 'npc_elder', 'ui_inventory_button', 'jump'.")]
        public string targetId;

        [Tooltip("How many times the condition must fire before the step is complete.")]
        public int requiredAmount = 1;

        [Header("Optional - for tutorial highlighting")]
        [Tooltip("If true, the tutorial highlighter will try to point at / glow the registered UI target with this targetId.")]
        public bool highlightTarget = false;
    }
}
