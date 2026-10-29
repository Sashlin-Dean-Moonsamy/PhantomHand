using System;

namespace QuestSystem
{
    /// <summary>
    /// Central static hub for gameplay events. Combat, inventory, dialogue, and movement
    /// systems fire these events WITHOUT knowing anything about quests. QuestManager (and
    /// TutorialManager) subscribe to them to decide if any active step cares.
    ///
    /// This keeps your quest system fully decoupled from your gameplay code.
    /// </summary>
    public static class GameEvents
    {
        // Gameplay
        public static event Action<string> OnEnemyKilled;
        public static event Action<string, int> OnItemCollected;
        public static event Action<string> OnLocationReached;
        public static event Action<string> OnNpcTalkedTo;
        public static event Action<string> OnItemDelivered;
        public static event Action<string> OnItemUsed;

        // Tutorial / input
        public static event Action<string> OnKeyPressed;
        public static event Action<string> OnUiClicked;
        public static event Action<string> OnMenuOpened;
        public static event Action<string> OnDirectionMoved;

        // --- Raise methods: call these from your gameplay code ---
        public static void RaiseEnemyKilled(string enemyId) => OnEnemyKilled?.Invoke(enemyId);
        public static void RaiseItemCollected(string itemId, int amount = 1) => OnItemCollected?.Invoke(itemId, amount);
        public static void RaiseLocationReached(string locationId) => OnLocationReached?.Invoke(locationId);
        public static void RaiseNpcTalkedTo(string npcId) => OnNpcTalkedTo?.Invoke(npcId);
        public static void RaiseItemDelivered(string itemId) => OnItemDelivered?.Invoke(itemId);
        public static void RaiseItemUsed(string itemId) => OnItemUsed?.Invoke(itemId);

        public static void RaiseKeyPressed(string actionId) => OnKeyPressed?.Invoke(actionId);
        public static void RaiseUiClicked(string uiId) => OnUiClicked?.Invoke(uiId);
        public static void RaiseMenuOpened(string menuId) => OnMenuOpened?.Invoke(menuId);
        public static void RaiseDirectionMoved(string directionId) => OnDirectionMoved?.Invoke(directionId);
    }
}
