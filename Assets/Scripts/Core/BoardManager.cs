using System.Collections;
using System.Collections.Generic;
using BoardDefence.Board;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Defence;
using BoardDefence.Enemy;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BoardDefence.Core
{

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

        private int _testLives = 3;
        private bool _isGameOver = false;

        public BoardDefence.UI.GameUIController _gameUIController;


        private void Awake()
        {
            _placedItems = new Dictionary<Vector2Int, DefenceItemBase>();
            _availableItems = new Dictionary<DefenceItemType, int>();
            _placementEnabled = true;

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
            
            _gameUIController.UpdateUI();

            if (_testLives <= 0)
            {
                _isGameOver = true;
                _isBattleActive = false;
                StopBattle();
            }
        }

        private void Update()
        {
	    	    if (UnityEngine.Input.GetMouseButtonDown(0))
	    	    {
	
	    	        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
	    	        {
	    	            // Click for uii
	    	        }
	    	        else
	    	        {
	    	            HandleClick();
	    	        }
	    	    }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectItemType(DefenceItemType.Type1);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectItemType(DefenceItemType.Type2);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectItemType(DefenceItemType.Type3);
            }

	    	    if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
	    	    {
	    	        if (GameManager.Instance != null)
	    	        {
	    	            GameManager.Instance.StartBattle();
	    	        }
	    	        else
	    	        {
	    	            StartBattle();
	    	        }
	    	    }

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }
        }

        public void RestartGame()
        {

            // Stop battle
            _isBattleActive = false;
            _isGameOver = false;
            _testLives = 3;
            _placementEnabled = true;
            _enemyCounter = 0;

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                Destroy(enemy);
            }

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

            _availableItems.Clear();
            _availableItems[DefenceItemType.Type1] = 10;
            _availableItems[DefenceItemType.Type2] = 10;
            _availableItems[DefenceItemType.Type3] = 10;

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
        }


        public void StartBattle()
        {
            _placementEnabled = false;
            Debug.Log("=== BATTLE STARTED! ===");

            foreach (var kvp in _placedItems)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.StartAttacking();
                }
            }

            if (GameManager.Instance != null && GameManager.Instance.LevelManager != null)
            {
                GameManager.Instance.LevelManager.StartSpawning();
            }
            else
            {
                SpawnTestEnemies();
            }
        }


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

            while (_isBattleActive)
            {
                int randomColumn = UnityEngine.Random.Range(0, 4);
                Vector2Int spawnPos = new Vector2Int(randomColumn, 0);
                Vector3 worldPos = _gameBoard.GridToWorldPosition(spawnPos);

                var enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                enemy.name = $"TestEnemy_{_enemyCounter++}";
                enemy.transform.position = worldPos;
                enemy.transform.localScale = Vector3.one * 0.6f;

                Color enemyColor = GetRandomEnemyColor();
                enemy.GetComponent<Renderer>().material.color = enemyColor;

                var mover = enemy.AddComponent<SimpleEnemyMover>();
                mover.Initialize(_gameBoard, spawnPos);


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


        public void StopBattle()
        {
            _isBattleActive = false;
            _placementEnabled = true;
            Debug.Log("=== BATTLE STOPPED ===");
        }


        public void InitializeForLevel(LevelData levelData)
        {
            _availableItems.Clear();
            
            foreach (var allocation in levelData.AvailableDefenceItems)
            {
                _availableItems[allocation.ItemType] = allocation.Count;
            }
            
            _placementEnabled = true;
        }


        public void SelectItemType(DefenceItemType type)
        {
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


        public void DeselectItemType()
        {
            _selectedItemType = null;
        }

 
        private void HandleClick()
        {
	    	    if (!_placementEnabled)
	    	        return;

            if (!_selectedItemType.HasValue)
            {
                return;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return;
                }
            }

            var mousePos = UnityEngine.Input.mousePosition;
            mousePos.z = Mathf.Abs(_mainCamera.transform.position.z);
            var worldPos = _mainCamera.ScreenToWorldPoint(mousePos);

            if (_gameBoard == null)
            {
                return;
            }

            var gridPos = _gameBoard.WorldToGridPosition(worldPos);

            TryPlaceItem(_selectedItemType.Value, gridPos);
        }

        public bool TryPlaceItem(DefenceItemType type, Vector2Int gridPosition)
        {
            if (!_availableItems.TryGetValue(type, out int count) || count <= 0)
            {
                return false;
            }

            if (!_gameBoard.IsPlaceablePosition(gridPosition))
            {
                return false;
            }

            var cell = _gameBoard.GetCell(gridPosition);
            if (cell == null || cell.IsOccupied)
            {
                return false;
            }

            var worldPos = _gameBoard.GridToWorldPosition(gridPosition);

            if (_defenceFactory == null)
            {
                var placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placeholder.transform.position = worldPos;
                placeholder.transform.localScale = Vector3.one * 0.8f;
                placeholder.GetComponent<Renderer>().material.color = GetColorForType(type);

                var attacker = placeholder.AddComponent<SimpleTurret>();
                attacker.Initialize(type, gridPosition);

	                cell.PlaceObject(placeholder);
	                _placedItems[gridPosition] = null;
	                _availableItems[type]--;
	                GameEvents.RaiseDefenceItemPlaced(gridPosition, type);
	                return true;
            }

	            var item = _defenceFactory.CreateAndPlace(type, gridPosition, worldPos, transform);

	            if (item != null)
	            {
	                cell.PlaceObject(item.gameObject);
	                _placedItems[gridPosition] = item;
	                _availableItems[type]--;
	                GameEvents.RaiseDefenceItemPlaced(gridPosition, type);
	                return true;
	            }

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
	            GameEvents.RaiseDefenceItemRemoved(gridPosition);
	            return true;
        }


        public void StartAllDefenceItems()
        {
            _placementEnabled = false;
            
            foreach (var item in _placedItems.Values)
            {
                item.StartAttacking();
            }
        }


        public void StopAllDefenceItems()
        {
            foreach (var item in _placedItems.Values)
            {
                item.StopAttacking();
            }
        }


        public void ClearAllDefenceItems()
        {
            var positions = new List<Vector2Int>(_placedItems.Keys);
            foreach (var pos in positions)
            {
                TryRemoveItem(pos);
            }
        }


        public int GetAvailableCount(DefenceItemType type)
        {
            return _availableItems.TryGetValue(type, out int count) ? count : 0;
        }


        public void SetPlacementEnabled(bool enabled)
        {
            _placementEnabled = enabled;
        }
    }
}

