using UnityEngine;

namespace BoardDefence.Core
{

    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Core Systems")]
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private BoardDefence.Board.GameBoard _gameBoard;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private BoardManager _boardManager;
        
        [Header("Factories & Pools")]
        [SerializeField] private Defence.DefenceItemFactory _defenceFactory;
        [SerializeField] private Enemy.EnemyPool _enemyPool;
        
        [Header("Settings")]
        [SerializeField] private bool _autoStartGame;

        private void Awake()
        {
            ValidateReferences();
            RegisterServices();
        }

        private void Start()
        {
            InitializeSystems();
            
            if (_autoStartGame)
            {
                _gameManager.StartGame();
            }
        }

        private void OnApplicationQuit()
        {
            ServiceLocator.SetQuitting(true);
            ServiceLocator.Clear();
        }

        private void ValidateReferences()
        {
            if (_gameManager == null)
                Debug.LogError("GameBootstrapper: GameManager reference is missing!");
            if (_gameBoard == null)
                Debug.LogError("GameBootstrapper: GameBoard reference is missing!");
            if (_levelManager == null)
                Debug.LogError("GameBootstrapper: LevelManager reference is missing!");
            if (_boardManager == null)
                Debug.LogError("GameBootstrapper: BoardManager reference is missing!");
            if (_defenceFactory == null)
                Debug.LogError("GameBootstrapper: DefenceItemFactory reference is missing!");
            if (_enemyPool == null)
                Debug.LogError("GameBootstrapper: EnemyPool reference is missing!");
        }

        private void RegisterServices()
        {
            // Register all services to ServiceLocator
            ServiceLocator.Register(_gameManager);
            ServiceLocator.Register(_gameBoard);
            ServiceLocator.Register(_levelManager);
            ServiceLocator.Register(_boardManager);
            ServiceLocator.Register(_defenceFactory);
            ServiceLocator.Register(_enemyPool);
        }

        private void InitializeSystems()
        {
            // Initialize in order
            _gameBoard.Initialize();
            
            Debug.Log("Board Defence: All systems initialized!");
        }
    }
}

