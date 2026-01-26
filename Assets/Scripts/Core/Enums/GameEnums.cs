namespace BoardDefence.Core.Enums
{

    public enum GameState
    {
        None,
        MainMenu,
        Preparation,    // Player placing defence items
        Battle,         // Enemies spawning and attacking
        Paused,
        Victory,
        Defeat
    }

    public enum DefenceItemType
    {
        Type1,  // damage: 3, range: 4, interval: 3s, direction: forward
        Type2,  // damage: 5, range: 2, interval: 4s, direction: forward
        Type3   // damage: 10, range: 1, interval: 5s, direction: all
    }


    public enum EnemyType
    {
        Type1,  // health: 3, speed: 1 block/s
        Type2,  // health: 10, speed: 0.25 block/s
        Type3   // health: 5, speed: 0.5 block/s
    }


    public enum AttackDirection
    {
        Forward,    // Only attacks in forward direction (up)
        All         // Attacks in all 4 directions
    }
}

