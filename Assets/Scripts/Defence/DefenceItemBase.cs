using System.Collections;
using System.Collections.Generic;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Defence.AttackStrategies;
using BoardDefence.Interfaces;
using BoardDefence.Enemy;
using UnityEngine;

namespace BoardDefence.Defence
{
    /// <summary>
    /// Base class for all defence items
    /// Uses Strategy Pattern for attack behavior
    /// Implements IPlaceable for board placement
    /// </summary>
    public class DefenceItemBase : MonoBehaviour, IPlaceable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        private DefenceItemData _data;
        private IAttackStrategy _attackStrategy;
        private Vector2Int _gridPosition;
        private bool _isPlaced;
        private bool _isAttacking;
        private Coroutine _attackCoroutine;

        #region IPlaceable Implementation
        
        public Vector2Int GridPosition => _gridPosition;
        public bool IsPlaced => _isPlaced;
        
        #endregion

        #region Properties
        
        public DefenceItemData Data => _data;
        public DefenceItemType ItemType => _data?.ItemType ?? DefenceItemType.Type1;
        public int Damage => _data?.Damage ?? 0;
        public int Range => _data?.Range ?? 0;
        public float AttackInterval => _data?.AttackInterval ?? 1f;
        
        #endregion

        /// <summary>
        /// Initialize the defence item with its data
        /// </summary>
        public void Initialize(DefenceItemData data)
        {
            _data = data;
            _attackStrategy = AttackStrategyFactory.GetStrategy(data.AttackDirection);
            
            if (_spriteRenderer != null && data.Sprite != null)
            {
                _spriteRenderer.sprite = data.Sprite;
                _spriteRenderer.color = data.TintColor;
            }
        }

        public void Place(Vector2Int position)
        {
            _gridPosition = position;
            _isPlaced = true;
            
            GameEvents.RaiseDefenceItemPlaced(position, ItemType);
        }

        public void Remove()
        {
            StopAttacking();
            _isPlaced = false;
            
            GameEvents.RaiseDefenceItemRemoved(_gridPosition);
            Destroy(gameObject);
        }

        public bool CanBePlacedAt(Vector2Int position)
        {
            // Defence items can only be placed in the bottom half (rows 4-7)
            return position.y >= 4 && position.y < 8 && position.x >= 0 && position.x < 4;
        }

        /// <summary>
        /// Start the attack loop
        /// </summary>
        public void StartAttacking()
        {
            if (_isAttacking) return;
            
            _isAttacking = true;
            _attackCoroutine = StartCoroutine(AttackLoop());
        }

        /// <summary>
        /// Stop the attack loop
        /// </summary>
        public void StopAttacking()
        {
            _isAttacking = false;
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }

        /// <summary>
        /// Main attack loop coroutine
        /// </summary>
        private IEnumerator AttackLoop()
        {
            while (_isAttacking)
            {
                PerformAttack();
                yield return new WaitForSeconds(AttackInterval);
            }
        }

        /// <summary>
        /// Perform an attack using the current strategy
        /// </summary>
        protected virtual void PerformAttack()
        {
            var targetPositions = _attackStrategy.GetTargetPositions(_gridPosition, Range);

            // Find enemies in range and shoot at them
            foreach (var pos in targetPositions)
            {
                var enemy = FindEnemyInDirection(pos);
                if (enemy != null)
                {
                    ShootProjectileAt(enemy);
                    break; // Only shoot one projectile per attack
                }
            }
        }

        /// <summary>
        /// Find enemy in the given direction
        /// </summary>
        protected virtual GameObject FindEnemyInDirection(Vector2Int targetPos)
        {
				Vector3 worldPos = GetWorldPosition(targetPos);
				
				// Search for enemies near this board cell first
				Collider[] colliders = Physics.OverlapSphere(worldPos, 0.5f);
				foreach (var col in colliders)
				{
					var damageable = col.GetComponent<IDamageable>();
					if (damageable != null)
					{
						var candidate = col.gameObject;
						if (!IsValidColumnForThisDefence(candidate))
							continue;
						return candidate;
					}
				}
				
				// Also check 2D colliders
				Collider2D[] colliders2D = Physics2D.OverlapCircleAll(worldPos, 0.5f);
				foreach (var col in colliders2D)
				{
					var damageable = col.GetComponent<IDamageable>();
					if (damageable != null)
					{
						var candidate = col.gameObject;
						if (!IsValidColumnForThisDefence(candidate))
							continue;
						return candidate;
					}
				}
				
				// Fallback: search all enemies but STILL respect direction (target cell)
				var enemies = GameObject.FindGameObjectsWithTag("Enemy");
				if (enemies.Length == 0)
				{
					// Fallback: find by name
					enemies = FindEnemiesByName();
				}
				
				GameObject closest = null;
				float closestDist = float.MaxValue;
				const float cellTolerance = 0.75f; // how close to the target cell the enemy must be
				
				foreach (var enemy in enemies)
				{
					float distanceToCell = Vector3.Distance(worldPos, enemy.transform.position);
					if (distanceToCell <= cellTolerance && distanceToCell < closestDist)
					{
						if (!IsValidColumnForThisDefence(enemy))
							continue;
						closest = enemy;
						closestDist = distanceToCell;
					}
				}
				
				return closest;
        }

	        /// <summary>
	        /// Forward (sadece ileri) savunmalar için: hedefin gerçekten aynı kolonda
	        /// olup olmadığını kontrol eder. Diğer yönlerde ateş edenler için kısıtlama yok.
	        /// </summary>
	        private bool IsValidColumnForThisDefence(GameObject enemy)
	        {
	            // Sadece Forward atak yönü olan item'lar için kolon kilidi uygula
	            if (_data != null && _data.AttackDirection == AttackDirection.Forward)
	            {
	                // Önce grid tabanlı kontrol (SimpleEnemyMover varsa)
	                var mover = enemy.GetComponent<SimpleEnemyMover>();
	                if (mover != null)
	                {
	                    if (mover.CurrentGridPos.x != _gridPosition.x)
	                        return false;
	                }
	                else
	                {
	                    // Yedek olarak world X'e bak
	                    float dx = Mathf.Abs(enemy.transform.position.x - transform.position.x);
	                    if (dx > 0.05f)
	                        return false;
	                }
	            }
	
	            return true;
	        }

        private GameObject[] FindEnemiesByName()
        {
            var allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            var enemies = new List<GameObject>();

            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Enemy") || obj.name.Contains("enemy"))
                {
                    enemies.Add(obj.gameObject);
                }
            }

            return enemies.ToArray();
        }

        /// <summary>
        /// Shoot a visual projectile at the target
        /// </summary>
        protected virtual void ShootProjectileAt(GameObject target)
        {
            if (target == null) return;

            // Create visual projectile
            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Bullet";
            projectile.transform.position = transform.position;
            projectile.transform.localScale = Vector3.one * 0.2f;

            // Set color based on defence type
            var renderer = projectile.GetComponent<Renderer>();
            renderer.material.color = GetProjectileColor();

            // Remove default collider and add trigger
            Destroy(projectile.GetComponent<Collider>());

            // Add bullet mover component
            var mover = projectile.AddComponent<BulletMover>();
            mover.Initialize(target, Damage, 8f);

            GameEvents.RaiseDefenceItemAttacked(_gridPosition, Damage);
        }

        private Color GetProjectileColor()
        {
            if (_data == null) return Color.yellow;

            return _data.ItemType switch
            {
                DefenceItemType.Type1 => Color.cyan,
                DefenceItemType.Type2 => Color.green,
                DefenceItemType.Type3 => Color.magenta,
                _ => Color.yellow
            };
        }

        /// <summary>
        /// Get world position from grid position (simplified - should use board reference)
        /// </summary>
        protected Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            // Simple conversion - actual implementation should reference GameBoard
            return new Vector3(gridPos.x - 1.5f, -(gridPos.y - 3.5f), 0);
        }
    }

    /// <summary>
    /// Simple bullet mover component
    /// </summary>
    public class BulletMover : MonoBehaviour
    {
        private GameObject _target;
        private int _damage;
        private float _speed;
        private Vector3 _lastTargetPos;
        private float _lifetime = 3f;

        public void Initialize(GameObject target, int damage, float speed)
        {
            _target = target;
            _damage = damage;
            _speed = speed;
            _lastTargetPos = target != null ? target.transform.position : transform.position + Vector3.up;
        }

        private void Update()
        {
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0)
            {
                Destroy(gameObject);
                return;
            }

            // Update target position if target still exists
            if (_target != null)
            {
                _lastTargetPos = _target.transform.position;
            }

            // Move towards target
            Vector3 direction = (_lastTargetPos - transform.position).normalized;
            transform.position += direction * _speed * Time.deltaTime;

            // Check if reached target
            if (Vector3.Distance(transform.position, _lastTargetPos) < 0.2f)
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            if (_target != null)
            {
                var damageable = _target.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsDead)
                {
                    damageable.TakeDamage(_damage);
                    Debug.Log($"Bullet hit! Dealt {_damage} damage");

                    // Visual hit effect
                    CreateHitEffect();
                }
            }

            Destroy(gameObject);
        }

        private void CreateHitEffect()
        {
            // Simple hit effect - flash
            var effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "HitEffect";
            effect.transform.position = transform.position;
            effect.transform.localScale = Vector3.one * 0.4f;
            effect.GetComponent<Renderer>().material.color = Color.white;
            Destroy(effect.GetComponent<Collider>());
            Destroy(effect, 0.15f);
        }
    }
}

