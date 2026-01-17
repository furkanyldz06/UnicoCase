using System;
using System.Collections.Generic;
using BoardDefence.Core.Enums;
using UnityEngine;

namespace BoardDefence.Data
{
    /// <summary>
    /// Data structure for spawn configuration
    /// </summary>
    [Serializable]
    public class SpawnData
    {
        public EnemyType EnemyType;
        [Min(1)] public int Count = 1;
        [Min(0f)] public float SpawnDelay = 1f; // Delay between each spawn
    }

    /// <summary>
    /// Data structure for available defence items in a level
    /// </summary>
    [Serializable]
    public class DefenceItemAllocation
    {
        public DefenceItemType ItemType;
        [Min(1)] public int Count = 1;
    }

    /// <summary>
    /// ScriptableObject for Level configuration
    /// Allows easy level design and modification as per requirements
    /// </summary>
    [CreateAssetMenu(fileName = "LevelData", menuName = "BoardDefence/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        [SerializeField] private int _levelNumber;
        [SerializeField] private string _levelName;
        [SerializeField] private string _description;

        [Header("Available Defence Items")]
        [SerializeField] private List<DefenceItemAllocation> _availableDefenceItems = new();

        [Header("Enemy Spawns")]
        [SerializeField] private List<SpawnData> _enemySpawns = new();
        
        [Header("Timing")]
        [SerializeField] [Min(1f)] private float _preparationTime = 30f; // Time to place defence items
        [SerializeField] [Min(0.5f)] private float _timeBetweenSpawns = 2f;
        [SerializeField] [Min(0f)] private float _initialSpawnDelay = 3f;

        #region Properties
        
        public int LevelNumber => _levelNumber;
        public string LevelName => _levelName;
        public string Description => _description;
        public IReadOnlyList<DefenceItemAllocation> AvailableDefenceItems => _availableDefenceItems;
        public IReadOnlyList<SpawnData> EnemySpawns => _enemySpawns;
        public float PreparationTime => _preparationTime;
        public float TimeBetweenSpawns => _timeBetweenSpawns;
        public float InitialSpawnDelay => _initialSpawnDelay;
        
        #endregion

        /// <summary>
        /// Get total enemy count for this level
        /// </summary>
        public int GetTotalEnemyCount()
        {
            int total = 0;
            foreach (var spawn in _enemySpawns)
            {
                total += spawn.Count;
            }
            return total;
        }

        /// <summary>
        /// Get count of specific defence item type available
        /// </summary>
        public int GetDefenceItemCount(DefenceItemType type)
        {
            foreach (var allocation in _availableDefenceItems)
            {
                if (allocation.ItemType == type)
                    return allocation.Count;
            }
            return 0;
        }

        /// <summary>
        /// Get count of specific enemy type to spawn
        /// </summary>
        public int GetEnemyCount(EnemyType type)
        {
            int count = 0;
            foreach (var spawn in _enemySpawns)
            {
                if (spawn.EnemyType == type)
                    count += spawn.Count;
            }
            return count;
        }
    }
}

