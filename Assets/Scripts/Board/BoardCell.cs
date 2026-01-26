using UnityEngine;

namespace BoardDefence.Board
{
    public class BoardCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _highlightColor = Color.yellow;
        [SerializeField] private Color _validPlacementColor = Color.green;
        [SerializeField] private Color _invalidPlacementColor = Color.red;
        [SerializeField] private Color _occupiedColor = Color.gray;
	        
	        [Header("Placement Zone Visuals")]
	        [SerializeField, Range(0f, 1f)] private float _nonPlaceableOpacity = 0.4f;
        
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

        public void Initialize(Vector2Int position, bool isPlaceableZone)
        {
            _gridPosition = position;
            _isPlaceableZone = isPlaceableZone;
            _isOccupied = false;
            _occupant = null;
            
            UpdateVisual();
        }

        public bool PlaceObject(GameObject obj)
        {
            if (!CanPlaceDefence)
                return false;

            _occupant = obj;
            _isOccupied = true;
            UpdateVisual();
            return true;
        }

        public GameObject RemoveObject()
        {
            var obj = _occupant;
            _occupant = null;
            _isOccupied = false;
            UpdateVisual();
            return obj;
        }
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
	            
	            if (!_isPlaceableZone)
	            {
	                var c = _spriteRenderer.color;
	                c.a = _nonPlaceableOpacity;
	                _spriteRenderer.color = c;
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

