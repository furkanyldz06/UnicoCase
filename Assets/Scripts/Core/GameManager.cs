using BoardDefence.Board;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Defence;
using BoardDefence.Enemy;
using UnityEngine;

namespace BoardDefence.Core
{

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private DefenceItemFactory _defenceFactory;
        [SerializeField] private EnemyPool _enemyPool;

        [Header("Settings")]
        [SerializeField] private int _playerLives = 3;

        private GameState _currentState = GameState.None;
        private int _currentLives;

        #region Properties
        
        public GameState CurrentState => _currentState;
        public int CurrentLives => _currentLives;
        public GameBoard Board => _gameBoard;
        public LevelManager LevelManager => _levelManager;
        public BoardManager BoardManager => _boardManager;
        public DefenceItemFactory DefenceFactory => _defenceFactory;
        public EnemyPool EnemyPool => _enemyPool;
        
        #endregion

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _currentLives = _playerLives;
        }

        private void Start()
        {
            SubscribeToEvents();
            Initialize();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnEnemyReachedBase += HandleEnemyReachedBase;
            GameEvents.OnAllEnemiesDefeated += HandleAllEnemiesDefeated;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnEnemyReachedBase -= HandleEnemyReachedBase;
            GameEvents.OnAllEnemiesDefeated -= HandleAllEnemiesDefeated;
        }


        public void Initialize()
        {
            _currentLives = _playerLives;
            _gameBoard.Initialize();
            
            SetState(GameState.MainMenu);
        }


        public void StartGame()
        {
	        if (_levelManager == null)
	            return;
	        
	        if (_levelManager.CurrentLevelData == null)
	        {
	            _levelManager.LoadLevel(1);
	        }
	        
	        _currentLives = _playerLives;
	        SetState(GameState.Preparation);
	        
	        GameEvents.RaiseGameStarted();
        }


        public void StartBattle()
        {
            if (_currentState != GameState.Preparation) return;
            
            SetState(GameState.Battle);
            _levelManager.StartSpawning();
            _boardManager.StartAllDefenceItems();
        }

        public void PauseGame()
        {
            if (_currentState == GameState.Battle || _currentState == GameState.Preparation)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
                GameEvents.RaiseGamePaused();
            }
        }


        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;
            
            Time.timeScale = 1f;
            SetState(GameState.Battle);
            GameEvents.RaiseGameResumed();
        }

        private void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            
            _currentState = newState;
            GameEvents.RaiseGameStateChanged(newState);
        }

        private void HandleEnemyReachedBase()
        {
            _currentLives--;
            
            if (_currentLives <= 0)
            {
                GameOver();
            }
        }

        private void HandleAllEnemiesDefeated()
        {
	        if (_currentState != GameState.Battle)
	            return;
	        
	        if (_levelManager == null)
	            return;
	        
	        if (_levelManager.HasNextLevel)
	        {
	            if (_boardManager != null)
	                _boardManager.ClearAllDefenceItems();
	            
	            _currentLives = _playerLives;
	            _levelManager.LoadNextLevel();
	            SetState(GameState.MainMenu);
	        }
	        else
	        {
	            Victory();
	        }
        }

        private void Victory()
        {
            SetState(GameState.Victory);
            _boardManager.StopAllDefenceItems();
            GameEvents.RaiseVictory();
        }

        private void GameOver()
        {
            SetState(GameState.Defeat);
            _boardManager.StopAllDefenceItems();
            _enemyPool.ReturnAll();
            GameEvents.RaiseGameOver();
        }

        public void RestartLevel()
        {
            _enemyPool.ReturnAll();
            _boardManager.ClearAllDefenceItems();
            _currentLives = _playerLives;
            _levelManager.ReloadCurrentLevel();
            SetState(GameState.Preparation);
        }
    }
}

