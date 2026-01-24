using System.Collections;
using System.Collections.Generic;
using BoardDefence.Board;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Enemy;
using UnityEngine;

namespace BoardDefence.Core
{
    /// <summary>
    /// Manages level loading, enemy spawning, and wave progression
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private EnemyPool _enemyPool;
        [SerializeField] private BoardManager _boardManager;

        [Header("Level Data")]
        [SerializeField] private LevelData[] _levels;

        private LevelData _currentLevelData;
        private int _currentLevelIndex;
        private int _totalEnemies;
        private int _defeatedEnemies;
        private int _escapedEnemies;
        private bool _isSpawning;
        private Coroutine _spawnCoroutine;

        #region Properties
        
        public LevelData CurrentLevelData => _currentLevelData;
        public int CurrentLevel => _currentLevelIndex + 1;
        public int TotalEnemies => _totalEnemies;
        public int DefeatedEnemies => _defeatedEnemies;
        public int RemainingEnemies => _totalEnemies - _defeatedEnemies - _escapedEnemies;
        public bool IsSpawning => _isSpawning;
        
        #endregion

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnEnemyDied += HandleEnemyDied;
            GameEvents.OnEnemyReachedBase += HandleEnemyReachedBase;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnEnemyDied -= HandleEnemyDied;
            GameEvents.OnEnemyReachedBase -= HandleEnemyReachedBase;
        }

        /// <summary>
        /// Load a specific level
        /// </summary>
        public void LoadLevel(int levelNumber)
        {
            int index = levelNumber - 1;
            
            if (index < 0 || index >= _levels.Length)
            {
                Debug.LogError($"Level {levelNumber} not found");
                return;
            }

            _currentLevelIndex = index;
            _currentLevelData = _levels[index];
            _totalEnemies = _currentLevelData.GetTotalEnemyCount();
            _defeatedEnemies = 0;
            _escapedEnemies = 0;

            _boardManager.InitializeForLevel(_currentLevelData);
            
            GameEvents.RaiseLevelStarted(levelNumber);
        }

        /// <summary>
        /// Reload the current level
        /// </summary>
        public void ReloadCurrentLevel()
        {
            LoadLevel(_currentLevelIndex + 1);
        }

        /// <summary>
        /// Start spawning enemies
        /// </summary>
        public void StartSpawning()
        {
            if (_isSpawning) return;
            
            _isSpawning = true;
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
            
            GameEvents.RaiseWaveStarted();
        }

        /// <summary>
        /// Stop spawning enemies
        /// </summary>
        public void StopSpawning()
        {
            _isSpawning = false;
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        /// <summary>
        /// Main spawn routine
        /// </summary>
        private IEnumerator SpawnRoutine()
        {
            // Null kontrolü
            if (_currentLevelData == null)
            {
                Debug.LogWarning("No level data set! Using default spawn values.");
                yield return StartCoroutine(DefaultSpawnRoutine());
                yield break;
            }

            yield return new WaitForSeconds(_currentLevelData.InitialSpawnDelay);

            foreach (var spawnData in _currentLevelData.EnemySpawns)
            {
                for (int i = 0; i < spawnData.Count; i++)
                {
                    SpawnEnemy(spawnData.EnemyType);
                    yield return new WaitForSeconds(_currentLevelData.TimeBetweenSpawns);
                }
            }

            _isSpawning = false;
            GameEvents.RaiseWaveCompleted();
        }

        /// <summary>
        /// Default spawn routine when no level data is available
        /// Düşmanlar sürekli olarak spawn olur (endless mode)
        /// </summary>
        private IEnumerator DefaultSpawnRoutine()
        {
            yield return new WaitForSeconds(1f);

            // Sürekli düşman spawn et - oyun bitene kadar
            while (_isSpawning)
            {
                SpawnEnemy(GetRandomEnemyType());

                // Rastgele spawn aralığı (1-3 saniye)
                float spawnInterval = Random.Range(1f, 3f);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// Rastgele düşman tipi seç
        /// </summary>
        private EnemyType GetRandomEnemyType()
        {
            int random = Random.Range(0, 3);
            return random switch
            {
                0 => EnemyType.Type1,
                1 => EnemyType.Type2,
                2 => EnemyType.Type3,
                _ => EnemyType.Type1
            };
        }

        /// <summary>
        /// Spawn a single enemy
        /// </summary>
        private void SpawnEnemy(EnemyType type)
        {
            // Null kontrolleri
            if (_enemyPool == null)
            {
                Debug.LogWarning("EnemyPool is null! Cannot spawn enemy.");
                SpawnTestEnemy();
                return;
            }

            if (_gameBoard == null)
            {
                Debug.LogError("GameBoard is null! Cannot spawn enemy.");
                return;
            }

            var enemy = _enemyPool.Get(type);
            if (enemy == null)
            {
                Debug.LogWarning($"Could not get enemy of type {type} from pool.");
                SpawnTestEnemy();
                return;
            }

            // Random column for spawn
            int column = Random.Range(0, _gameBoard.Width);
            var spawnPosition = new Vector2Int(column, 0);

            enemy.StartMoving(spawnPosition, _gameBoard.GridToWorldPosition);
        }

        /// <summary>
        /// Spawn test enemy when pool is not available
        /// </summary>
        private void SpawnTestEnemy()
        {
            if (_gameBoard == null) return;

            int column = Random.Range(0, _gameBoard.Width);
            Vector2Int spawnPos = new Vector2Int(column, 0);
            Vector3 worldPos = _gameBoard.GridToWorldPosition(spawnPos);

            // Basit düşman oluştur
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            enemy.name = $"TestEnemy_{_defeatedEnemies + _escapedEnemies}";
            enemy.transform.position = worldPos;
            enemy.transform.localScale = Vector3.one * 0.6f;
            enemy.GetComponent<Renderer>().material.color = Color.red;

            // Hareket scripti ekle
            var mover = enemy.AddComponent<BoardDefence.Enemy.SimpleEnemyMover>();
            mover.Initialize(_gameBoard, spawnPos);

            Debug.Log($"Spawned test enemy at column {column}");
        }

        private void HandleEnemyDied(Vector2Int position)
        {
            _defeatedEnemies++;
            CheckLevelComplete();
        }

        private void HandleEnemyReachedBase()
        {
            _escapedEnemies++;
            CheckLevelComplete();
        }

        private void CheckLevelComplete()
        {
            // All enemies spawned and defeated/escaped
            if (!_isSpawning && _defeatedEnemies + _escapedEnemies >= _totalEnemies)
            {
                if (_enemyPool.TotalActiveEnemies == 0)
                {
                    GameEvents.RaiseAllEnemiesDefeated();
                    GameEvents.RaiseLevelCompleted(_currentLevelIndex + 1);
                }
            }
        }

        /// <summary>
        /// Load next level if available
        /// </summary>
        public bool LoadNextLevel()
        {
            if (_currentLevelIndex + 1 < _levels.Length)
            {
                LoadLevel(_currentLevelIndex + 2);
                return true;
            }
            return false;
        }
    }
}

