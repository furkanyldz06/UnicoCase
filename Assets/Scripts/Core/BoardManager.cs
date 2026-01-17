using System.Collections.Generic;
using BoardDefence.Board;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Defence;
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

        private void Awake()
        {
            _placedItems = new Dictionary<Vector2Int, DefenceItemBase>();
            _availableItems = new Dictionary<DefenceItemType, int>();
            
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (_placementEnabled && UnityEngine.Input.GetMouseButtonDown(0))
            {
                HandleClick();
            }
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
            if (_availableItems.TryGetValue(type, out int count) && count > 0)
            {
                _selectedItemType = type;
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
            if (!_selectedItemType.HasValue) return;

            var worldPos = _mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            var gridPos = _gameBoard.WorldToGridPosition(worldPos);
            
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
            var item = _defenceFactory.CreateAndPlace(type, gridPosition, worldPos, transform);
            
            if (item != null)
            {
                cell.PlaceObject(item.gameObject);
                _placedItems[gridPosition] = item;
                _availableItems[type]--;
                
                return true;
            }

            return false;
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

