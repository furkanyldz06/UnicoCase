using BoardDefence.Core;
using BoardDefence.Core.Enums;
using BoardDefence.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BoardDefence.UI
{
    /// <summary>
    /// UI component for defence item selection buttons
    /// </summary>
    public class DefenceItemButton : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private DefenceItemType _itemType;
        [SerializeField] private DefenceItemData _itemData;
        
        [Header("UI Elements")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _selectionHighlight;
        
        [Header("Colors")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = Color.yellow;
        [SerializeField] private Color _disabledColor = Color.gray;

        private bool _isSelected;

        private void Awake()
        {
            if (_button == null)
            {
                _button = GetComponent<Button>();
            }
            
            _button?.onClick.AddListener(OnClick);
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            _button?.onClick.RemoveListener(OnClick);
        }

        /// <summary>
        /// Initialize the button with item data
        /// </summary>
        public void Initialize()
        {
            if (_itemData != null)
            {
                if (_iconImage != null && _itemData.Sprite != null)
                {
                    _iconImage.sprite = _itemData.Sprite;
                }
                
                if (_nameText != null)
                {
                    _nameText.text = _itemData.ItemName;
                }
            }
            
            UpdateDisplay();
        }

        /// <summary>
        /// Update the button display
        /// </summary>
        public void UpdateDisplay()
        {
            var boardManager = GameManager.Instance?.BoardManager;
            if (boardManager == null) return;

            int count = boardManager.GetAvailableCount(_itemType);
            
            if (_countText != null)
            {
                _countText.text = count.ToString();
            }

            bool isAvailable = count > 0 && boardManager.PlacementEnabled;
            
            if (_button != null)
            {
                _button.interactable = isAvailable;
            }

            UpdateVisuals(isAvailable);
        }

        private void UpdateVisuals(bool isAvailable)
        {
            if (_iconImage != null)
            {
                if (!isAvailable)
                {
                    _iconImage.color = _disabledColor;
                }
                else if (_isSelected)
                {
                    _iconImage.color = _selectedColor;
                }
                else
                {
                    _iconImage.color = _normalColor;
                }
            }

            if (_selectionHighlight != null)
            {
                _selectionHighlight.enabled = _isSelected;
            }
        }

        /// <summary>
        /// Set the selection state of this button
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateDisplay();
        }

        private void OnClick()
        {
            var boardManager = GameManager.Instance?.BoardManager;
            if (boardManager == null) return;

            boardManager.SelectItemType(_itemType);
            
            // Notify other buttons to deselect
            var allButtons = FindObjectsOfType<DefenceItemButton>();
            foreach (var button in allButtons)
            {
                button.SetSelected(button == this);
            }
        }
    }
}

