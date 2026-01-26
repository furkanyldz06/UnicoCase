using System;
using BoardDefence.Core.Enums;
using UnityEngine;

namespace BoardDefence.Core.Events
{

    public static class GameEvents
    {
        // Game State Events
        public static event Action<GameState> OnGameStateChanged;
        public static event Action OnGameStarted;
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;
        public static event Action OnGameOver;
        public static event Action OnVictory;

        // Level Events
        public static event Action<int> OnLevelStarted;
        public static event Action<int> OnLevelCompleted;
        public static event Action OnAllEnemiesDefeated;
        public static event Action OnWaveStarted;
        public static event Action OnWaveCompleted;

        // Defence Item Events
        public static event Action<Vector2Int, DefenceItemType> OnDefenceItemPlaced;
        public static event Action<Vector2Int> OnDefenceItemRemoved;
        public static event Action<Vector2Int, int> OnDefenceItemAttacked;

        // Enemy Events
        public static event Action<Vector2Int, EnemyType> OnEnemySpawned;
        public static event Action<Vector2Int> OnEnemyDied;
        public static event Action<Vector2Int, int> OnEnemyDamaged;
        public static event Action OnEnemyReachedBase;

        // Board Events
        public static event Action<Vector2Int> OnCellSelected;
        public static event Action OnBoardCleared;

        #region Event Invokers
        
        public static void RaiseGameStateChanged(GameState state) => OnGameStateChanged?.Invoke(state);
        public static void RaiseGameStarted() => OnGameStarted?.Invoke();
        public static void RaiseGamePaused() => OnGamePaused?.Invoke();
        public static void RaiseGameResumed() => OnGameResumed?.Invoke();
        public static void RaiseGameOver() => OnGameOver?.Invoke();
        public static void RaiseVictory() => OnVictory?.Invoke();

        public static void RaiseLevelStarted(int level) => OnLevelStarted?.Invoke(level);
        public static void RaiseLevelCompleted(int level) => OnLevelCompleted?.Invoke(level);
        public static void RaiseAllEnemiesDefeated() => OnAllEnemiesDefeated?.Invoke();
        public static void RaiseWaveStarted() => OnWaveStarted?.Invoke();
        public static void RaiseWaveCompleted() => OnWaveCompleted?.Invoke();

        public static void RaiseDefenceItemPlaced(Vector2Int pos, DefenceItemType type) 
            => OnDefenceItemPlaced?.Invoke(pos, type);
        public static void RaiseDefenceItemRemoved(Vector2Int pos) 
            => OnDefenceItemRemoved?.Invoke(pos);
        public static void RaiseDefenceItemAttacked(Vector2Int pos, int damage) 
            => OnDefenceItemAttacked?.Invoke(pos, damage);

        public static void RaiseEnemySpawned(Vector2Int pos, EnemyType type) 
            => OnEnemySpawned?.Invoke(pos, type);
        public static void RaiseEnemyDied(Vector2Int pos) => OnEnemyDied?.Invoke(pos);
        public static void RaiseEnemyDamaged(Vector2Int pos, int damage) 
            => OnEnemyDamaged?.Invoke(pos, damage);
        public static void RaiseEnemyReachedBase() => OnEnemyReachedBase?.Invoke();

        public static void RaiseCellSelected(Vector2Int pos) => OnCellSelected?.Invoke(pos);
        public static void RaiseBoardCleared() => OnBoardCleared?.Invoke();

        #endregion

        public static void ClearAllEvents()
        {
            OnGameStateChanged = null;
            OnGameStarted = null;
            OnGamePaused = null;
            OnGameResumed = null;
            OnGameOver = null;
            OnVictory = null;
            OnLevelStarted = null;
            OnLevelCompleted = null;
            OnAllEnemiesDefeated = null;
            OnWaveStarted = null;
            OnWaveCompleted = null;
            OnDefenceItemPlaced = null;
            OnDefenceItemRemoved = null;
            OnDefenceItemAttacked = null;
            OnEnemySpawned = null;
            OnEnemyDied = null;
            OnEnemyDamaged = null;
            OnEnemyReachedBase = null;
            OnCellSelected = null;
            OnBoardCleared = null;
        }
    }
}

