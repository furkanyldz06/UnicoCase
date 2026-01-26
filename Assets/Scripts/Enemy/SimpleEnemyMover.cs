using BoardDefence.Board;
using BoardDefence.Core.Events;
using BoardDefence.Interfaces;
using UnityEngine;

namespace BoardDefence.Enemy
{

    public class SimpleEnemyMover : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _moveSpeed = 1f; // blok/saniye
        [SerializeField] private int _maxHealth = 3;
        
        private GameBoard _gameBoard;
        private Vector2Int _currentGridPos;
        private Vector3 _targetWorldPos;
        private int _currentHealth;
	        private bool _isMoving = true;
	        private bool _isDead = false;

	        public Vector2Int CurrentGridPos => _currentGridPos;

	        #region IDamageable
	        public int CurrentHealth => _currentHealth;
	        public int MaxHealth => _maxHealth;
	        public bool IsDead => _isDead;
	        #endregion

        public void Initialize(GameBoard board, Vector2Int startPos)
        {
            _gameBoard = board;
            _currentGridPos = startPos;
            _currentHealth = _maxHealth;
            _targetWorldPos = transform.position;

            gameObject.tag = "Enemy";

            if (GetComponent<SphereCollider>() == null)
            {
                var collider = gameObject.AddComponent<SphereCollider>();
                collider.radius = 0.3f;
                collider.isTrigger = true;
            }

            SetNextTarget();
        }

        private void Update()
        {
            if (_isDead || !_isMoving) return;

            float step = _moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _targetWorldPos, step);

            if (Vector3.Distance(transform.position, _targetWorldPos) < 0.01f)
            {
                _currentGridPos = new Vector2Int(_currentGridPos.x, _currentGridPos.y + 1);
                
                if (_currentGridPos.y >= 8)
                {
                    ReachedBase();
                    return;
                }
                
                SetNextTarget();
            }
        }

        private void SetNextTarget()
        {
            Vector2Int nextPos = new Vector2Int(_currentGridPos.x, _currentGridPos.y + 1);
            
            if (_gameBoard != null)
            {
                _targetWorldPos = _gameBoard.GridToWorldPosition(nextPos);
            }
            else
            {
                _targetWorldPos = transform.position + Vector3.down;
            }
        }

        private void ReachedBase()
        {
            _isMoving = false;
            Debug.Log($"Enemy reached base at column {_currentGridPos.x}!");
            GameEvents.RaiseEnemyReachedBase();
            Destroy(gameObject, 0.5f);
        }

        #region IDamageable Methods
        public void TakeDamage(int damage)
        {
            if (_isDead) return;
            
            _currentHealth -= damage;
            Debug.Log($"Enemy took {damage} damage! Health: {_currentHealth}/{_maxHealth}");
            
            StartCoroutine(DamageFlash());
            
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            _isDead = true;
            _isMoving = false;
            Debug.Log("Enemy died!");
            GameEvents.RaiseEnemyDied(_currentGridPos);
            
            GetComponent<Renderer>().material.color = Color.black;
            Destroy(gameObject, 0.3f);
        }
        #endregion

        private System.Collections.IEnumerator DamageFlash()
        {
            var renderer = GetComponent<Renderer>();
            if (renderer == null) yield break;
            
            Color originalColor = renderer.material.color;
            renderer.material.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            if (!_isDead)
                renderer.material.color = originalColor;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
           
        }
    }
}

