using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    /// <summary>
    /// A full quest (or tutorial) definition: metadata, ordered steps, prerequisites, and rewards.
    /// Create instances via Assets > Create > Quests > Quest.
    /// </summary>
    [CreateAssetMenu(menuName = "Quests/Quest", fileName = "Quest_NewQuest")]
    public class QuestSO : ScriptableObject
    {
        [Tooltip("Unique id for this quest, used for save/load and lookups. Keep stable once shipped.")]
        public string questId;

        [Tooltip("If true, this quest starts automatically when the QuestManager loads.")]
public bool autoStart;

        public string title;

        [TextArea]
        public string description;

        [Tooltip("Steps must be completed in order.")]
        public List<QuestStepSO> steps = new List<QuestStepSO>();

        [Tooltip("Quests that must be Completed before this one can start.")]
        public List<QuestSO> prerequisites = new List<QuestSO>();

        public List<RewardData> rewards = new List<RewardData>();

        [Header("Flags")]
        [Tooltip("If true, this quest is treated as a tutorial and hidden from the normal quest log UI.")]
        public bool isTutorial = false;

        [Tooltip("If true (and isTutorial), this starts automatically the first time the game is played.")]
        public bool autoStartOnFirstPlay = false;

        /// <summary>Checks if all prerequisite quests are completed according to the given manager.</summary>
        public bool PrerequisitesMet(QuestManager manager)
        {
            foreach (var prereq in prerequisites)
            {
                if (prereq == null) continue;
                if (!manager.IsQuestCompleted(prereq.questId))
                    return false;
            }
            return true;
        }
    }
}
