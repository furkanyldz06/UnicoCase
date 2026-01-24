using System.Collections;
using System.Collections.Generic;
using BoardDefence.Core.Enums;
using BoardDefence.Interfaces;
using BoardDefence.Board;
using BoardDefence.Enemy;
using UnityEngine;

namespace BoardDefence.Defence
{
    /// <summary>
    /// Simple turret component for placeholder defence items
    /// Automatically shoots at nearby enemies
    /// </summary>
    public class SimpleTurret : MonoBehaviour
    {
        private DefenceItemType _type;
        private Vector2Int _gridPosition;
        private float _attackInterval;
        private int _damage;
        private float _range;
	        // true = sadece ileri (yukarı); false = her yön
	        private bool _forwardOnly;
        private bool _isAttacking;
        private Coroutine _attackCoroutine;

	        public void Initialize(DefenceItemType type, Vector2Int gridPos)
	        {
	            _type = type;
	            _gridPosition = gridPos;
	
	            // Spec'e göre değerler
	            // Range = kaç hücre ilerisine ateş edebilir (1 hücre ≈ 1 birim)
	            switch (type)
	            {
	                case DefenceItemType.Type1:
	                    _damage = 3;
	                    _range = 4f;
	                    _attackInterval = 3f;
	                    _forwardOnly = true;  // sadece ileri (yukarı)
	                    break;
	                case DefenceItemType.Type2:
	                    _damage = 5;
	                    _range = 2f;
	                    _attackInterval = 4f;
	                    _forwardOnly = true;  // sadece ileri (yukarı)
	                    break;
	                case DefenceItemType.Type3:
	                    _damage = 10;
	                    _range = 1f;
	                    _attackInterval = 5f;
	                    _forwardOnly = false; // her yön
	                    break;
	            }
	
	            // Otomatik saldırıya başla
	            StartAttacking();
	        }

        public void StartAttacking()
        {
            if (_isAttacking) return;
            _isAttacking = true;
            _attackCoroutine = StartCoroutine(AttackLoop());
        }

        public void StopAttacking()
        {
            _isAttacking = false;
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }

        private IEnumerator AttackLoop()
        {
            yield return new WaitForSeconds(0.5f); // Initial delay
            
            while (_isAttacking)
            {
                TryAttack();
                yield return new WaitForSeconds(_attackInterval);
            }
        }

        private void TryAttack()
        {
            // Find nearest enemy
            GameObject nearestEnemy = FindNearestEnemy();
            
            if (nearestEnemy != null)
            {
                ShootAt(nearestEnemy);
            }
        }

		private GameObject FindNearestEnemy()
		{
		    GameObject nearest = null;
		    float nearestDist = float.MaxValue;
		
		    var enemies = GameObject.FindGameObjectsWithTag("Enemy");
		
			GameBoard board = null;
			if (_forwardOnly)
			{
			    board = GameObject.FindObjectOfType<GameBoard>();
			}
		
		    foreach (var enemy in enemies)
		    {
		        var damageable = enemy.GetComponent<IDamageable>();
		        if (damageable == null || damageable.IsDead)
		            continue;
		
		        Vector3 enemyPos = enemy.transform.position;
		        Vector3 delta = enemyPos - transform.position;
		        float dist = delta.magnitude;
		
		        if (dist > _range + 0.5f)
		            continue;
		
			        if (_forwardOnly)
			        {
			            bool sameColumn;
			            bool isAbove;
			
			            // Önce SimpleEnemyMover üzerinden grid pozisyonunu dene
			            var mover = enemy.GetComponent<SimpleEnemyMover>();
			            if (mover != null)
			            {
			                Vector2Int enemyGrid = mover.CurrentGridPos;
			                sameColumn = enemyGrid.x == _gridPosition.x;
			                isAbove = enemyGrid.y < _gridPosition.y;
			            }
			            else if (board != null)
			            {
			                // Fallback: board'dan world->grid çevir
			                Vector2Int enemyGrid = board.WorldToGridPosition(enemyPos);
			                sameColumn = enemyGrid.x == _gridPosition.x;
			                isAbove = enemyGrid.y < _gridPosition.y;
			            }
			            else
			            {
			                // Son çare: dünya koordinatı ile kabaca kontrol
			                float absX = Mathf.Abs(delta.x);
			                sameColumn = absX < 0.3f;
			                isAbove = delta.y > 0.1f;
			            }
			
			            if (!sameColumn || !isAbove)
			                continue;
			        }
		
		        if (dist < nearestDist)
		        {
		            nearest = enemy;
		            nearestDist = dist;
		        }
		    }
		
		    return nearest;
		}

        private void ShootAt(GameObject target)
        {
            // Create bullet
            var bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Bullet";
            bullet.transform.position = transform.position;
            bullet.transform.localScale = Vector3.one * 0.25f;
            
            // Set color
            var renderer = bullet.GetComponent<Renderer>();
            renderer.material.color = GetBulletColor();
            
            // Remove collider
            Destroy(bullet.GetComponent<Collider>());
            
            // Add mover
            var mover = bullet.AddComponent<BulletMover>();
            mover.Initialize(target, _damage, 10f);
            
            Debug.Log($"Turret at {_gridPosition} fired at {target.name}!");
        }

        private Color GetBulletColor()
        {
            return _type switch
            {
                DefenceItemType.Type1 => Color.cyan,
                DefenceItemType.Type2 => Color.green,
                DefenceItemType.Type3 => Color.magenta,
                _ => Color.yellow
            };
        }

        private void OnDestroy()
        {
            StopAttacking();
        }
    }
}

