using UnityEngine;

namespace BoardDefence.Board
{
    /// <summary>
    /// Represents a single cell on the game board
    /// Each cell can hold either a defence item or be a path for enemies
    /// </summary>
    public class BoardCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private Color _validPlacementColor = Color.green;
        [SerializeField] private Color _invalidPlacementColor = Color.red;
        [SerializeField] private Color _occupiedColor = Color.gray;
        
        private Vector2Int _gridPosition;
        private bool _isPlaceableZone;
        private bool _isOccupied;
        private GameObject _occupant;

        #region Properties
        
        public Vector2Int GridPosition => _gridPosition;
        public bool IsPlaceableZone => _isPlaceableZone;
        public bool IsOccupied => _isOccupied;
        public GameObject Occupant => _occupant;
        public bool CanPlaceDefence => _isPlaceableZone && !_isOccupied;
        
        #endregion

        /// <summary>
        /// Initialize the cell with its position and zone type
        /// </summary>
        public void Initialize(Vector2Int position, bool isPlaceableZone)
        {
            _gridPosition = position;
            _isPlaceableZone = isPlaceableZone;
            _isOccupied = false;
            _occupant = null;
            
            UpdateVisual();
        }

        /// <summary>
        /// Place an object on this cell
        /// </summary>
        public bool PlaceObject(GameObject obj)
        {
            if (!CanPlaceDefence)
                return false;

            _occupant = obj;
            _isOccupied = true;
            UpdateVisual();
            return true;
        }

        /// <summary>
        /// Remove the occupant from this cell
        /// </summary>
        public GameObject RemoveObject()
        {
            var obj = _occupant;
            _occupant = null;
            _isOccupied = false;
            UpdateVisual();
            return obj;
        }

        /// <summary>
        /// Highlight the cell for placement preview
        /// </summary>
        public void SetHighlight(bool highlighted)
        {
            if (highlighted)
            {
                _spriteRenderer.color = CanPlaceDefence ? _validPlacementColor : _invalidPlacementColor;
            }
            else
            {
                UpdateVisual();
            }
        }

        /// <summary>
        /// Select/deselect this cell
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selected)
            {
                _spriteRenderer.color = _highlightColor;
            }
            else
            {
                UpdateVisual();
            }
        }

        private void UpdateVisual()
        {
            if (_spriteRenderer == null) return;

            if (_isOccupied)
            {
                _spriteRenderer.color = _occupiedColor;
            }
            else
            {
                _spriteRenderer.color = _normalColor;
            }
        }

        private void OnMouseEnter()
        {
            if (_isPlaceableZone && !_isOccupied)
            {
                SetHighlight(true);
            }
        }

        private void OnMouseExit()
        {
            SetHighlight(false);
        }
    }
}

