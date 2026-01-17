using BoardDefence.Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace BoardDefence.Utils
{
    /// <summary>
    /// Simple health bar UI component
    /// Can be attached to any IDamageable entity
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private bool _hideWhenFull = true;
        [SerializeField] private Color _fullHealthColor = Color.green;
        [SerializeField] private Color _midHealthColor = Color.yellow;
        [SerializeField] private Color _lowHealthColor = Color.red;
        [SerializeField] private float _lowHealthThreshold = 0.3f;
        [SerializeField] private float _midHealthThreshold = 0.6f;

        private IDamageable _target;
        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponentInChildren<Canvas>();
        }

        private void Start()
        {
            // Try to find target on parent
            _target = GetComponentInParent<IDamageable>();
            UpdateDisplay();
        }

        private void Update()
        {
            if (_target != null)
            {
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Set the target to track
        /// </summary>
        public void SetTarget(IDamageable target)
        {
            _target = target;
            UpdateDisplay();
        }

        /// <summary>
        /// Update the health bar display
        /// </summary>
        public void UpdateDisplay()
        {
            if (_target == null || _fillImage == null) return;

            float healthPercent = (float)_target.CurrentHealth / _target.MaxHealth;
            _fillImage.fillAmount = healthPercent;

            // Update color based on health
            if (healthPercent <= _lowHealthThreshold)
            {
                _fillImage.color = _lowHealthColor;
            }
            else if (healthPercent <= _midHealthThreshold)
            {
                _fillImage.color = _midHealthColor;
            }
            else
            {
                _fillImage.color = _fullHealthColor;
            }

            // Hide when full
            if (_hideWhenFull && _canvas != null)
            {
                _canvas.enabled = healthPercent < 1f;
            }
        }
    }
}

