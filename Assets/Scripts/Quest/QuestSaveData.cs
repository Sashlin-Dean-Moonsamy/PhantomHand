using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuestSystem
{
    /// <summary>
    /// Serializable snapshot of a single quest's progress, safe to write to JSON/binary saves.
    /// </summary>
    [Serializable]
    public class QuestSaveEntry
    {
        public string questId;
        public int currentStepIndex;
        public int currentProgress;
        public QuestStatus status;
    }

    [Serializable]
    public class QuestSaveData
    {
        public List<QuestSaveEntry> entries = new List<QuestSaveEntry>();
    }

    /// <summary>
    /// Static helper to convert QuestManager's runtime state to/from a serializable
    /// snapshot. Hook these calls into your existing save/load pipeline
    /// (e.g. call CaptureState() when writing a save file, and ApplyState() after loading one).
    /// </summary>
    public static class QuestSaveSystem
    {
        public static QuestSaveData CaptureState(QuestManager manager)
        {
            var data = new QuestSaveData();
            foreach (var state in manager.GetAllStates())
            {
                data.entries.Add(new QuestSaveEntry
                {
                    questId = state.quest.questId,
                    currentStepIndex = state.currentStepIndex,
                    currentProgress = state.currentProgress,
                    status = state.status
                });
            }
            return data;
        }

        public static void ApplyState(QuestManager manager, QuestSaveData data)
        {
            if (data == null) return;

            foreach (var entry in data.entries)
            {
                var state = manager.GetState(entry.questId);
                if (state == null)
                {
                    Debug.LogWarning($"[QuestSaveSystem] Save references unknown quest '{entry.questId}', skipping.");
                    continue;
                }

                state.currentStepIndex = entry.currentStepIndex;
                state.currentProgress = entry.currentProgress;
                state.status = entry.status;
            }
        }

        // Example JSON round-trip helpers using Unity's built-in JsonUtility.
        public static string ToJson(QuestManager manager) =>
            JsonUtility.ToJson(CaptureState(manager), true);

        public static void FromJson(QuestManager manager, string json)
        {
            var data = JsonUtility.FromJson<QuestSaveData>(json);
            ApplyState(manager, data);
        }
    }
}
