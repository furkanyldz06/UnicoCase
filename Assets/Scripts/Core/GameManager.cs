using BoardDefence.Board;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Defence;
using BoardDefence.Enemy;
using UnityEngine;

namespace BoardDefence.Core
{
    /// <summary>
    /// Main game manager implementing State Pattern for game flow
    /// Acts as the central coordinator between systems
    /// Uses Dependency Injection pattern via serialized fields
    /// </summary>
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

        /// <summary>
        /// Initialize all game systems
        /// </summary>
        public void Initialize()
        {
            _currentLives = _playerLives;
            _gameBoard.Initialize();
            
            SetState(GameState.MainMenu);
        }

        /// <summary>
        /// Start the game with the first level
        /// </summary>
        public void StartGame()
        {
            _currentLives = _playerLives;
            _levelManager.LoadLevel(1);
            SetState(GameState.Preparation);
            
            GameEvents.RaiseGameStarted();
        }

        /// <summary>
        /// Start the battle phase
        /// </summary>
        public void StartBattle()
        {
            if (_currentState != GameState.Preparation) return;
            
            SetState(GameState.Battle);
            _levelManager.StartSpawning();
            _boardManager.StartAllDefenceItems();
        }

        /// <summary>
        /// Pause the game
        /// </summary>
        public void PauseGame()
        {
            if (_currentState == GameState.Battle || _currentState == GameState.Preparation)
            {
                SetState(GameState.Paused);
                Time.timeScale = 0f;
                GameEvents.RaiseGamePaused();
            }
        }

        /// <summary>
        /// Resume the game
        /// </summary>
        public void ResumeGame()
        {
            if (_currentState != GameState.Paused) return;
            
            Time.timeScale = 1f;
            SetState(GameState.Battle);
            GameEvents.RaiseGameResumed();
        }

        /// <summary>
        /// Set the game state
        /// </summary>
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
	    	    // Sadece aktif savaş halindeyken anlamlı
	    	    if (_currentState != GameState.Battle)
	    	        return;
	    	
	    	    int finishedLevel = _levelManager != null ? _levelManager.CurrentLevel : -1;
	    	
	    	    // Tüm düşmanlar ölmüş ve halen canımız varsa: level BAŞARILI
	    	    // Sonraki level varsa otomatik geç; yoksa klasik Victory ekranı.
	    	    if (_levelManager != null && _levelManager.LoadNextLevel())
	    	    {
	    	        Debug.Log($"[GameManager] Level {finishedLevel} SUCCESS. Loading next level: {_levelManager.CurrentLevel}");
	    	        // Yeni level için tahtayı temizle, canları resetle ve tekrar hazırlığa geç
	    	        _boardManager.ClearAllDefenceItems();
	    	        _currentLives = _playerLives;
	    	        SetState(GameState.Preparation);
	    	    }
	    	    else
	    	    {
	    	        // Son leveli de bitirdik: artık level yok, tam Victory
	    	        Debug.Log($"[GameManager] Last level {finishedLevel} SUCCESS. Game Victory.");
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

        /// <summary>
        /// Restart the current level
        /// </summary>
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

