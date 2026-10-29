using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    /// <summary>
    /// Full quest log panel. Subscribes to QuestManager events and rebuilds its list
    /// of entries whenever quests start, advance, or complete.
    ///
    /// Tutorial quests (QuestSO.isTutorial == true) are skipped so they don't clutter
    /// the player-facing quest log.
    /// </summary>
    public class QuestLogUI : MonoBehaviour
    {
        [SerializeField] private Transform entryContainer;
        [SerializeField] private QuestLogEntryUI entryPrefab;
        [SerializeField] private bool showCompletedQuests = true;

        private readonly Dictionary<string, QuestLogEntryUI> spawnedEntries = new Dictionary<string, QuestLogEntryUI>();

        void OnEnable()
        {
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.OnQuestStarted += HandleQuestChanged;
            QuestManager.Instance.OnStepAdvanced += HandleStepAdvanced;
            QuestManager.Instance.OnQuestCompleted += HandleQuestChanged;

            RebuildAll();
        }

        void OnDisable()
        {
            if (QuestManager.Instance == null) return;

            QuestManager.Instance.OnQuestStarted -= HandleQuestChanged;
            QuestManager.Instance.OnStepAdvanced -= HandleStepAdvanced;
            QuestManager.Instance.OnQuestCompleted -= HandleQuestChanged;
        }

        void HandleQuestChanged(QuestSO quest) => RebuildAll();

        void HandleStepAdvanced(QuestState state)
        {
            if (spawnedEntries.TryGetValue(state.quest.questId, out var entry))
                entry.Bind(state);
        }

        void RebuildAll()
        {
            foreach (Transform child in entryContainer)
                Destroy(child.gameObject);
            spawnedEntries.Clear();

            foreach (var state in QuestManager.Instance.GetAllStates())
            {
                if (state.quest.isTutorial) continue;
                if (state.status == QuestStatus.NotStarted) continue;
                if (state.status == QuestStatus.Completed && !showCompletedQuests) continue;

                var entry = Instantiate(entryPrefab, entryContainer);
                entry.Bind(state);
                spawnedEntries[state.quest.questId] = entry;
            }
        }
    }
}
