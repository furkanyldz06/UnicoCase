using System.Collections;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Interfaces;
using UnityEngine;

namespace BoardDefence.Enemy
{
    /// <summary>
    /// Base class for all enemies
    /// Implements IDamageable and IPoolable for damage system and object pooling
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyBase : MonoBehaviour, IDamageable, IPoolable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private EnemyData _data;
        private int _currentHealth;
        private bool _isActive;
        private bool _isMoving;
        private Vector2Int _currentGridPosition;
        private Coroutine _moveCoroutine;

        // Callback for when enemy is defeated or reaches base
        public System.Action<EnemyBase> OnDefeated;
        public System.Action<EnemyBase> OnReachedBase;

        #region IDamageable Implementation
        
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _data?.MaxHealth ?? 1;
        public bool IsDead => _currentHealth <= 0;
        
        #endregion

        #region IPoolable Implementation
        
        public bool IsActive => _isActive;
        
        #endregion

        #region Properties
        
        public EnemyData Data => _data;
        public EnemyType EnemyType => _data?.EnemyType ?? EnemyType.Type1;
        public float MoveSpeed => _data?.MoveSpeed ?? 1f;
        public Vector2Int CurrentGridPosition => _currentGridPosition;
        
        #endregion

        /// <summary>
        /// Initialize the enemy with its data
        /// </summary>
        public void Initialize(EnemyData data)
        {
            _data = data;
            ResetState();
            
            if (_spriteRenderer != null && data.Sprite != null)
            {
                _spriteRenderer.sprite = data.Sprite;
                _spriteRenderer.color = data.TintColor;
            }
        }

        #region IDamageable Methods
        
        public void TakeDamage(int damage)
        {
            if (IsDead) return;
            
            _currentHealth -= damage;
            
            GameEvents.RaiseEnemyDamaged(_currentGridPosition, damage);
            
            // Visual feedback
            StartCoroutine(DamageFlash());
            
            if (IsDead)
            {
                Die();
            }
        }

        public void Die()
        {
            StopMoving();
            _isActive = false;
            
            GameEvents.RaiseEnemyDied(_currentGridPosition);
            OnDefeated?.Invoke(this);
        }
        
        #endregion

        #region IPoolable Methods
        
        public void OnSpawn()
        {
            _isActive = true;
            gameObject.SetActive(true);
            ResetState();
        }

        public void OnDespawn()
        {
            _isActive = false;
            StopMoving();
            gameObject.SetActive(false);
        }

        public void ResetState()
        {
            _currentHealth = MaxHealth;
            _isMoving = false;
            
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = _data?.TintColor ?? Color.white;
            }
        }
        
        #endregion

        /// <summary>
        /// Start moving the enemy towards the base
        /// </summary>
        public void StartMoving(Vector2Int startPosition, System.Func<Vector2Int, Vector3> gridToWorld)
        {
            _currentGridPosition = startPosition;
            transform.position = gridToWorld(startPosition);
            
            _isMoving = true;
            _moveCoroutine = StartCoroutine(MoveRoutine(gridToWorld));
            
            GameEvents.RaiseEnemySpawned(startPosition, EnemyType);
        }

        /// <summary>
        /// Stop the enemy's movement
        /// </summary>
        public void StopMoving()
        {
            _isMoving = false;
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
        }

        /// <summary>
        /// Movement coroutine - moves from top to bottom
        /// </summary>
        private IEnumerator MoveRoutine(System.Func<Vector2Int, Vector3> gridToWorld)
        {
            while (_isMoving && !IsDead)
            {
                // Move to next row (down = positive Y in grid)
                var nextPosition = new Vector2Int(_currentGridPosition.x, _currentGridPosition.y + 1);
                
                // Check if reached base (bottom of board)
                if (nextPosition.y >= 8) // Board height
                {
                    ReachedBase();
                    yield break;
                }

                // Calculate movement
                var startPos = transform.position;
                var endPos = gridToWorld(nextPosition);
                float moveTime = 1f / MoveSpeed; // Time to move one block
                float elapsed = 0f;

                while (elapsed < moveTime && _isMoving && !IsDead)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / moveTime);
                    transform.position = Vector3.Lerp(startPos, endPos, t);
                    yield return null;
                }

                if (_isMoving && !IsDead)
                {
                    _currentGridPosition = nextPosition;
                    transform.position = endPos;
                }
            }
        }

        /// <summary>
        /// Called when enemy reaches the player's base
        /// </summary>
        private void ReachedBase()
        {
            StopMoving();
            _isActive = false;
            
            GameEvents.RaiseEnemyReachedBase();
            OnReachedBase?.Invoke(this);
        }

        /// <summary>
        /// Visual feedback when taking damage
        /// </summary>
        private IEnumerator DamageFlash()
        {
            if (_spriteRenderer == null) yield break;
            
            var originalColor = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = originalColor;
        }
    }
}

