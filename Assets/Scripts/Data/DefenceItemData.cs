using BoardDefence.Core.Enums;
using UnityEngine;

namespace BoardDefence.Data
{
    /// <summary>
    /// ScriptableObject for Defence Item configuration
    /// Data-Driven Design for easy balancing and modification
    /// </summary>
    [CreateAssetMenu(fileName = "DefenceItemData", menuName = "BoardDefence/Defence Item Data")]
    public class DefenceItemData : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private DefenceItemType _itemType;
        [SerializeField] private string _itemName;
        [SerializeField] private string _description;
        
        [Header("Combat Stats")]
        [SerializeField] [Min(1)] private int _damage = 1;
        [SerializeField] [Min(1)] private int _range = 1;
        [SerializeField] [Min(0.1f)] private float _attackInterval = 1f;
        [SerializeField] private AttackDirection _attackDirection = AttackDirection.Forward;
        
        [Header("Visuals")]
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _tintColor = Color.white;
        [SerializeField] private GameObject _prefab;

        #region Properties
        
        public DefenceItemType ItemType => _itemType;
        public string ItemName => _itemName;
        public string Description => _description;
        public int Damage => _damage;
        public int Range => _range;
        public float AttackInterval => _attackInterval;
        public AttackDirection AttackDirection => _attackDirection;
        public Sprite Sprite => _sprite;
        public Color TintColor => _tintColor;
        public GameObject Prefab => _prefab;
        
        #endregion

        /// <summary>
        /// Validate data in editor
        /// </summary>
        private void OnValidate()
        {
            if (_damage < 1) _damage = 1;
            if (_range < 1) _range = 1;
            if (_attackInterval < 0.1f) _attackInterval = 0.1f;
        }
    }
}

