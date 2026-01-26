using UnityEngine;

namespace BoardDefence.Core
{

    public static class GameConstants
    {
        #region Board Configuration
        
        public const int BOARD_WIDTH = 4;
        public const int BOARD_HEIGHT = 8;
        public const int PLACEABLE_ROW_START = 4; // Bottom half (rows 4-7)
        
        #endregion

        #region Defence Item Stats
        
        // Type 1: damage: 3, range: 4 blocks, interval: 3s, direction: forward
        public const int DEFENCE_TYPE1_DAMAGE = 3;
        public const int DEFENCE_TYPE1_RANGE = 4;
        public const float DEFENCE_TYPE1_INTERVAL = 3f;
        
        // Type 2: damage: 5, range: 2 blocks, interval: 4s, direction: forward
        public const int DEFENCE_TYPE2_DAMAGE = 5;
        public const int DEFENCE_TYPE2_RANGE = 2;
        public const float DEFENCE_TYPE2_INTERVAL = 4f;
        
        // Type 3: damage: 10, range: 1 block, interval: 5s, direction: all
        public const int DEFENCE_TYPE3_DAMAGE = 10;
        public const int DEFENCE_TYPE3_RANGE = 1;
        public const float DEFENCE_TYPE3_INTERVAL = 5f;
        
        #endregion

        #region Enemy Stats
        
        // Type 1: health: 3, speed: 1 block/s
        public const int ENEMY_TYPE1_HEALTH = 3;
        public const float ENEMY_TYPE1_SPEED = 1f;
        
        // Type 2: health: 10, speed: 0.25 block/s
        public const int ENEMY_TYPE2_HEALTH = 10;
        public const float ENEMY_TYPE2_SPEED = 0.25f;
        
        // Type 3: health: 5, speed: 0.5 block/s
        public const int ENEMY_TYPE3_HEALTH = 5;
        public const float ENEMY_TYPE3_SPEED = 0.5f;
        
        #endregion

        #region Level 1 Configuration
        
        // Defence Items: 3x Type1, 2x Type2, 1x Type3
        public const int LEVEL1_DEFENCE_TYPE1 = 3;
        public const int LEVEL1_DEFENCE_TYPE2 = 2;
        public const int LEVEL1_DEFENCE_TYPE3 = 1;
        
        // Enemies: 3x Enemy1, 1x Enemy2, 1x Enemy3
        public const int LEVEL1_ENEMY_TYPE1 = 3;
        public const int LEVEL1_ENEMY_TYPE2 = 1;
        public const int LEVEL1_ENEMY_TYPE3 = 1;
        
        #endregion

        #region Level 2 Configuration
        
        // Defence Items: 3x Type1, 4x Type2, 2x Type3
        public const int LEVEL2_DEFENCE_TYPE1 = 3;
        public const int LEVEL2_DEFENCE_TYPE2 = 4;
        public const int LEVEL2_DEFENCE_TYPE3 = 2;
        
        // Enemies: 5x Enemy1, 2x Enemy2, 3x Enemy3
        public const int LEVEL2_ENEMY_TYPE1 = 5;
        public const int LEVEL2_ENEMY_TYPE2 = 2;
        public const int LEVEL2_ENEMY_TYPE3 = 3;
        
        #endregion

        #region Level 3 Configuration
        
        // Defence Items: 5x Type1, 7x Type2, 5x Type3
        public const int LEVEL3_DEFENCE_TYPE1 = 5;
        public const int LEVEL3_DEFENCE_TYPE2 = 7;
        public const int LEVEL3_DEFENCE_TYPE3 = 5;
        
        // Enemies: 7x Enemy1, 3x Enemy2, 5x Enemy3
        public const int LEVEL3_ENEMY_TYPE1 = 7;
        public const int LEVEL3_ENEMY_TYPE2 = 3;
        public const int LEVEL3_ENEMY_TYPE3 = 5;
        
        #endregion

        #region Timing
        
        public const float DEFAULT_PREPARATION_TIME = 30f;
        public const float DEFAULT_SPAWN_DELAY = 3f;
        public const float DEFAULT_SPAWN_INTERVAL = 2f;
        
        #endregion

        #region Visuals
        
        public static readonly Color CELL_NORMAL_COLOR = new(0.5f, 0.7f, 0.9f, 1f);
        public static readonly Color CELL_PLACEABLE_COLOR = new(0.6f, 0.8f, 0.6f, 1f);
        public static readonly Color CELL_HIGHLIGHT_COLOR = new(1f, 1f, 0.5f, 1f);
        
        #endregion
    }
}

