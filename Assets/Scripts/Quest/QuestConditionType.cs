namespace QuestSystem
{
    /// <summary>
    /// Defines what kind of game event a quest step is waiting for.
    /// Add new values here as your game grows (both for quests and tutorials).
    /// </summary>
    public enum QuestConditionType
    {
        // --- Gameplay / quest conditions ---
        Kill,
        Collect,
        ReachLocation,
        TalkTo,
        DeliverItem,
        UseItem,
        Custom,

        // --- Tutorial-specific conditions ---
        PressKey,
        ClickUI,
        OpenMenu,
        MoveInDirection,
        WaitForSeconds
    }
}
