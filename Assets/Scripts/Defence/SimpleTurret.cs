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

	            // Menzil çemberini oyun içinde göster
	            var rangeViz = GetComponent<AttackRangeVisualizer>();
	            if (rangeViz == null)
	            {
	                rangeViz = gameObject.AddComponent<AttackRangeVisualizer>();
	            }
	            rangeViz.Initialize(_range);

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
			
				    foreach (var enemy in enemies)
				    {
				        var damageable = enemy.GetComponent<IDamageable>();
				        if (damageable == null || damageable.IsDead)
				            continue;
				
					// Forward turret'ler için: sadece SimpleEnemyMover kullanan
					// grid tabanlı düşmanları ve SADECE aynı kolon + yukarıyı düşün.
					var mover = enemy.GetComponent<SimpleEnemyMover>();
					Vector2Int enemyGrid = mover != null ? mover.CurrentGridPos : new Vector2Int(-1, -1);
				
				        // Mesafe hesabı – forward turret'ler için kolonu world X'ten kilitle,
				        // diğerleri için klasik world mesafesi kullan.
				        float dist;
				
					if (_forwardOnly)
					{
					    if (mover == null)
					        continue; // grid bilgisiz düşmanı tamamen yok say
					
					    // 1) GRID'e göre aynı kolon mu?
					    //    Aynı kolonda değilse forward turret ASLA ateş etmesin.
					    if (enemyGrid.x != _gridPosition.x)
					        continue;
					
					    // 2) Sadece yukarıda (ileri yönde) olanlar
					    if (enemyGrid.y >= _gridPosition.y)
					        continue;
					
					    // 3) Menzil: kaç satır yukarıda?
					    int rowDelta = _gridPosition.y - enemyGrid.y; // 1,2,3,...
					    if (rowDelta > _range)
					        continue; // menzil dışında
					
					    dist = rowDelta; // forward için "mesafe" = grid satır farkı
					}
				        else
				        {
				            // All-direction turret'ler için klasik world mesafesi ve yarıçap
				            dist = (enemy.transform.position - transform.position).magnitude;
				            if (dist > _range + 0.5f)
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

