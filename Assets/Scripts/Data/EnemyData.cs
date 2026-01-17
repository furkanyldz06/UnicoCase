using BoardDefence.Core.Enums;
using UnityEngine;

namespace BoardDefence.Data
{
    /// <summary>
    /// ScriptableObject for Enemy configuration
    /// Data-Driven Design for easy balancing and modification
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "BoardDefence/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identification")]
        [SerializeField] private EnemyType _enemyType;
        [SerializeField] private string _enemyName;
        [SerializeField] private string _description;
        
        [Header("Stats")]
        [SerializeField] [Min(1)] private int _maxHealth = 1;
        [SerializeField] [Min(0.1f)] private float _moveSpeed = 1f; // blocks per second
        
        [Header("Visuals")]
        [SerializeField] private Sprite _sprite;
        [SerializeField] private Color _tintColor = Color.white;
        [SerializeField] private GameObject _prefab;
        
        [Header("Audio")]
        [SerializeField] private AudioClip _spawnSound;
        [SerializeField] private AudioClip _hitSound;
        [SerializeField] private AudioClip _deathSound;

        #region Properties
        
        public EnemyType EnemyType => _enemyType;
        public string EnemyName => _enemyName;
        public string Description => _description;
        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public Sprite Sprite => _sprite;
        public Color TintColor => _tintColor;
        public GameObject Prefab => _prefab;
        public AudioClip SpawnSound => _spawnSound;
        public AudioClip HitSound => _hitSound;
        public AudioClip DeathSound => _deathSound;
        
        #endregion

        /// <summary>
        /// Validate data in editor
        /// </summary>
        private void OnValidate()
        {
            if (_maxHealth < 1) _maxHealth = 1;
            if (_moveSpeed < 0.1f) _moveSpeed = 0.1f;
        }
    }
}

