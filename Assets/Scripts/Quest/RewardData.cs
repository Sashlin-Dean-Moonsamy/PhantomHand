using System;
using UnityEngine;

namespace QuestSystem
{
    public enum RewardType
    {
        Item,
        Experience,
        Currency
    }

    /// <summary>
    /// A single reward granted when a quest completes.
    /// Shows up nicely in the Inspector inside a QuestSO's reward list.
    /// </summary>
    [Serializable]
    public class RewardData
    {
        public RewardType rewardType;

        [Tooltip("Item id if rewardType is Item (must match your inventory system's item ids).")]
        public string itemId;

        [Tooltip("Amount of item / xp / currency to grant.")]
        public int amount = 1;
    }
}
