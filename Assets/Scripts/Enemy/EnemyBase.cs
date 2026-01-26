using System.Collections;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Interfaces;
using UnityEngine;

namespace BoardDefence.Enemy
{

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


        public void StartMoving(Vector2Int startPosition, System.Func<Vector2Int, Vector3> gridToWorld)
        {
            _currentGridPosition = startPosition;
            transform.position = gridToWorld(startPosition);
            
            _isMoving = true;
            _moveCoroutine = StartCoroutine(MoveRoutine(gridToWorld));
            
            GameEvents.RaiseEnemySpawned(startPosition, EnemyType);
        }


        public void StopMoving()
        {
            _isMoving = false;
            if (_moveCoroutine != null)
            {
                StopCoroutine(_moveCoroutine);
                _moveCoroutine = null;
            }
        }

        private IEnumerator MoveRoutine(System.Func<Vector2Int, Vector3> gridToWorld)
        {
            while (_isMoving && !IsDead)
            {
                var nextPosition = new Vector2Int(_currentGridPosition.x, _currentGridPosition.y + 1);
                
                if (nextPosition.y >= 8)
                {
                    ReachedBase();
                    yield break;
                }

                var startPos = transform.position;
                var endPos = gridToWorld(nextPosition);
                float moveTime = 1f / MoveSpeed;
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
        private void ReachedBase()
        {
            StopMoving();
            _isActive = false;
            
            GameEvents.RaiseEnemyReachedBase();
            OnReachedBase?.Invoke(this);
        }

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

