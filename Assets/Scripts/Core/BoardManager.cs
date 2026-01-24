using System.Collections;
using System.Collections.Generic;
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
    /// Manages defence item placement on the board
    /// Handles player interaction with the board
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private DefenceItemFactory _defenceFactory;
        [SerializeField] private Camera _mainCamera;

        [Header("Placement Settings")]
        [SerializeField] private LayerMask _boardLayerMask;

        private Dictionary<Vector2Int, DefenceItemBase> _placedItems;
        private Dictionary<DefenceItemType, int> _availableItems;
        private DefenceItemType? _selectedItemType;
        private bool _placementEnabled;

        #region Properties
        
        public bool PlacementEnabled => _placementEnabled;
        public DefenceItemType? SelectedItemType => _selectedItemType;
        public IReadOnlyDictionary<Vector2Int, DefenceItemBase> PlacedItems => _placedItems;
        
        #endregion

        // Test mode lives system
        private int _testLives = 3;
        private bool _isGameOver = false;

        private void Awake()
        {
            _placedItems = new Dictionary<Vector2Int, DefenceItemBase>();
            _availableItems = new Dictionary<DefenceItemType, int>();
            _placementEnabled = true; // Default olarak aktif

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyReachedBase += OnEnemyReachedBase;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyReachedBase -= OnEnemyReachedBase;
        }

        private void OnEnemyReachedBase()
        {
            if (_isGameOver) return;

            _testLives--;
            Debug.Log($"<color=red>💔 ENEMY REACHED BASE! Lives remaining: {_testLives}</color>");

            if (_testLives <= 0)
            {
                _isGameOver = true;
                _isBattleActive = false;
                Debug.Log("<color=red>========== GAME OVER! ==========</color>");
                Debug.Log("<color=yellow>Press R to restart</color>");
                StopBattle();
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }

            // Klavye ile item seçimi
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectItemType(DefenceItemType.Type1);
                Debug.Log("Selected: Type1");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectItemType(DefenceItemType.Type2);
                Debug.Log("Selected: Type2");
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectItemType(DefenceItemType.Type3);
                Debug.Log("Selected: Type3");
            }

            // SPACE tuşu ile savaşı başlat
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                StartBattle();
            }

            // R tuşu ile restart
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
        }

        /// <summary>
        /// Restart the game
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("=== RESTARTING GAME ===");

            // Stop battle
            _isBattleActive = false;
            _isGameOver = false;
            _testLives = 3;
            _placementEnabled = true;
            _enemyCounter = 0;

            // Destroy all enemies
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                Destroy(enemy);
            }

            // Also find by name as fallback
            var allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            foreach (var obj in allObjects)
            {
                if (obj != null && obj.name.Contains("Enemy"))
                {
                    Destroy(obj.gameObject);
                }
                if (obj != null && obj.name.Contains("Bullet"))
                {
                    Destroy(obj.gameObject);
                }
            }

            // Clear placed items
            foreach (var kvp in _placedItems)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _placedItems.Clear();

            // Clear placeholder turrets
            var turrets = FindObjectsByType<SimpleTurret>(FindObjectsSortMode.None);
            foreach (var turret in turrets)
            {
                Destroy(turret.gameObject);
            }

            // Reset available items
            _availableItems.Clear();
            _availableItems[DefenceItemType.Type1] = 10;
            _availableItems[DefenceItemType.Type2] = 10;
            _availableItems[DefenceItemType.Type3] = 10;

            // Reset cell colors
            if (_gameBoard != null)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 4; y < 8; y++)
                    {
                        var cell = _gameBoard.GetCell(new Vector2Int(x, y));
                        if (cell != null)
                        {
                            cell.RemoveObject();
                        }
                    }
                }
            }

            Debug.Log("Game restarted! Press 1/2/3 to select defence, click to place, SPACE to start battle.");
        }

        /// <summary>
        /// Savaşı başlat - tüm defence item'lar saldırmaya başlar
        /// </summary>
        public void StartBattle()
        {
            _placementEnabled = false;
            Debug.Log("=== BATTLE STARTED! ===");
            Debug.Log($"Placed {_placedItems.Count} defence items");

            // Tüm yerleştirilmiş savunma öğelerini aktif et
            foreach (var kvp in _placedItems)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.StartAttacking();
                    Debug.Log($"Defence at {kvp.Key} started attacking");
                }
            }

            // LevelManager varsa düşman spawn'ı başlat
            if (GameManager.Instance != null && GameManager.Instance.LevelManager != null)
            {
                GameManager.Instance.LevelManager.StartSpawning();
            }
            else
            {
                Debug.Log("LevelManager not found - spawning test enemies");
                SpawnTestEnemies();
            }
        }

        /// <summary>
        /// Test için basit düşmanlar spawn et
        /// </summary>
        private void SpawnTestEnemies()
        {
            if (_gameBoard == null) return;

            StartCoroutine(SpawnTestEnemiesCoroutine());
        }

        private bool _isBattleActive = false;
        private int _enemyCounter = 0;

        private System.Collections.IEnumerator SpawnTestEnemiesCoroutine()
        {
            _isBattleActive = true;

            // Sürekli düşman spawn et - savaş bitene kadar
            while (_isBattleActive)
            {
                int randomColumn = UnityEngine.Random.Range(0, 4);
                Vector2Int spawnPos = new Vector2Int(randomColumn, 0);
                Vector3 worldPos = _gameBoard.GridToWorldPosition(spawnPos);

                // Basit düşman oluştur
                var enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                enemy.name = $"TestEnemy_{_enemyCounter++}";
                enemy.transform.position = worldPos;
                enemy.transform.localScale = Vector3.one * 0.6f;

                // Rastgele düşman rengi (tipi)
                Color enemyColor = GetRandomEnemyColor();
                enemy.GetComponent<Renderer>().material.color = enemyColor;

                // Basit hareket scripti ekle
                var mover = enemy.AddComponent<SimpleEnemyMover>();
                mover.Initialize(_gameBoard, spawnPos);

                Debug.Log($"Spawned test enemy #{_enemyCounter} at column {randomColumn}");

                // Rastgele spawn aralığı (1-3 saniye)
                float spawnInterval = UnityEngine.Random.Range(1f, 3f);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private Color GetRandomEnemyColor()
        {
            int random = UnityEngine.Random.Range(0, 3);
            return random switch
            {
                0 => Color.red,      // Type1 - hızlı, düşük can
                1 => new Color(0.5f, 0f, 0.5f), // Type2 - yavaş, yüksek can (mor)
                2 => Color.yellow,   // Type3 - orta
                _ => Color.red
            };
        }

        /// <summary>
        /// Savaşı durdur
        /// </summary>
        public void StopBattle()
        {
            _isBattleActive = false;
            _placementEnabled = true;
            Debug.Log("=== BATTLE STOPPED ===");
        }

        /// <summary>
        /// Initialize available items for the current level
        /// </summary>
        public void InitializeForLevel(LevelData levelData)
        {
            _availableItems.Clear();
            
            foreach (var allocation in levelData.AvailableDefenceItems)
            {
                _availableItems[allocation.ItemType] = allocation.Count;
            }
            
            _placementEnabled = true;
        }

        /// <summary>
        /// Select an item type for placement
        /// </summary>
        public void SelectItemType(DefenceItemType type)
        {
            // Eğer availableItems boşsa, test için varsayılan değerler ekle
            if (_availableItems.Count == 0)
            {
                _availableItems[DefenceItemType.Type1] = 10;
                _availableItems[DefenceItemType.Type2] = 10;
                _availableItems[DefenceItemType.Type3] = 10;
            }

            if (_availableItems.TryGetValue(type, out int count) && count > 0)
            {
                _selectedItemType = type;
                Debug.Log($"Item selected: {type}, Available: {count}");
            }
            else
            {
                Debug.Log($"Cannot select {type}, count: {count}");
            }
        }

        /// <summary>
        /// Deselect the current item type
        /// </summary>
        public void DeselectItemType()
        {
            _selectedItemType = null;
        }

        /// <summary>
        /// Handle click for placement
        /// </summary>
        private void HandleClick()
        {
            if (!_selectedItemType.HasValue)
            {
                Debug.Log("No item type selected! Press 1, 2, or 3 to select.");
                return;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    Debug.LogError("Main Camera not found!");
                    return;
                }
            }

            var mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = Mathf.Abs(_mainCamera.transform.position.z);
            var worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

            if (_gameBoard == null)
            {
                Debug.LogError("GameBoard reference is missing!");
                return;
            }

            var gridPos = _gameBoard.WorldToGridPosition(worldPos);
            Debug.Log($"Click at world: {worldPos}, grid: {gridPos}");

            TryPlaceItem(_selectedItemType.Value, gridPos);
        }

        /// <summary>
        /// Try to place an item at the specified position
        /// </summary>
        public bool TryPlaceItem(DefenceItemType type, Vector2Int gridPosition)
        {
            // Check if we have available items
            if (!_availableItems.TryGetValue(type, out int count) || count <= 0)
            {
                Debug.Log($"No more {type} items available");
                return false;
            }

            // Check if position is valid
            if (!_gameBoard.IsPlaceablePosition(gridPosition))
            {
                Debug.Log($"Position {gridPosition} is not in the placeable zone");
                return false;
            }

            // Check if cell is occupied
            var cell = _gameBoard.GetCell(gridPosition);
            if (cell == null || cell.IsOccupied)
            {
                Debug.Log($"Cell at {gridPosition} is occupied or doesn't exist");
                return false;
            }

            // Create and place the item
            var worldPos = _gameBoard.GridToWorldPosition(gridPosition);

            if (_defenceFactory == null)
            {
                Debug.LogWarning("DefenceFactory is null! Creating simple placeholder with attack.");
                // Basit bir placeholder oluştur
                var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.transform.position = worldPos;
                placeholder.transform.localScale = Vector3.one * 0.8f;
                placeholder.GetComponent<Renderer>().material.color = GetColorForType(type);

                // Saldırı komponenti ekle
                var attacker = placeholder.AddComponent<SimpleTurret>();
                attacker.Initialize(type, gridPosition);

                cell.PlaceObject(placeholder);
                _placedItems[gridPosition] = null;
                _availableItems[type]--;
                Debug.Log($"Placed placeholder turret at {gridPosition}");
                return true;
            }

            var item = _defenceFactory.CreateAndPlace(type, gridPosition, worldPos, transform);

            if (item != null)
            {
                cell.PlaceObject(item.gameObject);
                _placedItems[gridPosition] = item;
                _availableItems[type]--;
                Debug.Log($"Successfully placed {type} at {gridPosition}");
                return true;
            }

            Debug.LogError("Failed to create defence item!");
            return false;
        }

        private Color GetColorForType(DefenceItemType type)
        {
            return type switch
            {
                DefenceItemType.Type1 => Color.blue,
                DefenceItemType.Type2 => Color.green,
                DefenceItemType.Type3 => Color.red,
                _ => Color.white
            };
        }

        /// <summary>
        /// Remove an item from the specified position
        /// </summary>
        public bool TryRemoveItem(Vector2Int gridPosition)
        {
            if (!_placedItems.TryGetValue(gridPosition, out var item))
            {
                return false;
            }

            var cell = _gameBoard.GetCell(gridPosition);
            cell?.RemoveObject();
            
            _availableItems[item.ItemType]++;
            _placedItems.Remove(gridPosition);
            
            item.Remove();
            return true;
        }

        /// <summary>
        /// Start all placed defence items attacking
        /// </summary>
        public void StartAllDefenceItems()
        {
            _placementEnabled = false;
            
            foreach (var item in _placedItems.Values)
            {
                item.StartAttacking();
            }
        }

        /// <summary>
        /// Stop all defence items from attacking
        /// </summary>
        public void StopAllDefenceItems()
        {
            foreach (var item in _placedItems.Values)
            {
                item.StopAttacking();
            }
        }

        /// <summary>
        /// Clear all placed defence items
        /// </summary>
        public void ClearAllDefenceItems()
        {
            var positions = new List<Vector2Int>(_placedItems.Keys);
            foreach (var pos in positions)
            {
                TryRemoveItem(pos);
            }
        }

        /// <summary>
        /// Get remaining count of specific item type
        /// </summary>
        public int GetAvailableCount(DefenceItemType type)
        {
            return _availableItems.TryGetValue(type, out int count) ? count : 0;
        }

        /// <summary>
        /// Enable or disable placement mode
        /// </summary>
        public void SetPlacementEnabled(bool enabled)
        {
            _placementEnabled = enabled;
        }
    }
}

