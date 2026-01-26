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


       public void Initialize(DefenceItemData data)
        {
            _data = data;
            _attackStrategy = AttackStrategyFactory.GetStrategy(data.AttackDirection);

            if (_spriteRenderer != null && data.Sprite != null)
            {
                _spriteRenderer.sprite = data.Sprite;
                _spriteRenderer.color = data.TintColor;
            }

            var rangeViz = GetComponent<AttackRangeVisualizer>();
            if (rangeViz == null)
            {
                rangeViz = gameObject.AddComponent<AttackRangeVisualizer>();
            }

            rangeViz.ForwardOnly = data.AttackDirection == AttackDirection.Forward;

            int rangeCells = _data != null ? _data.Range : 1;
            rangeViz.Initialize(rangeCells);
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
            return position.y >= 4 && position.y < 8 && position.x >= 0 && position.x < 4;
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
            while (_isAttacking)
            {
                PerformAttack();
                yield return new WaitForSeconds(AttackInterval);
            }
        }


        protected virtual void PerformAttack()
        {
            var targetPositions = _attackStrategy.GetTargetPositions(_gridPosition, Range);

            foreach (var pos in targetPositions)
            {
                var enemy = FindEnemyInDirection(pos);
                if (enemy != null)
                {
                    ShootProjectileAt(enemy);
                    break;
                }
            }
        }


        protected virtual GameObject FindEnemyInDirection(Vector2Int targetPos)
        {
				Vector3 worldPos = GetWorldPosition(targetPos);
				
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
				
				var enemies = GameObject.FindGameObjectsWithTag("Enemy");
				if (enemies.Length == 0)
				{
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

	        private bool IsValidColumnForThisDefence(GameObject enemy)
	        {
	            if (_data != null && _data.AttackDirection == AttackDirection.Forward)
	            {
	                var mover = enemy.GetComponent<SimpleEnemyMover>();
	                if (mover != null)
	                {
	                    if (mover.CurrentGridPos.x != _gridPosition.x)
	                        return false;
	                }
	                else
	                {
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


        protected virtual void ShootProjectileAt(GameObject target)
        {
            if (target == null) return;

            var projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "Bullet";
            projectile.transform.position = transform.position;
            projectile.transform.localScale = Vector3.one * 0.2f;

            var renderer = projectile.GetComponent<Renderer>();
            renderer.material.color = GetProjectileColor();

            Destroy(projectile.GetComponent<Collider>());

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


        protected Vector3 GetWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x - 1.5f, -(gridPos.y - 3.5f), 0);
        }
    }


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

            if (_target != null)
            {
                _lastTargetPos = _target.transform.position;
            }

            Vector3 direction = (_lastTargetPos - transform.position).normalized;
            transform.position += direction * _speed * Time.deltaTime;

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
	                    ShowDamageNumber(_damage);

	                    CreateHitEffect();
                }
            }

            Destroy(gameObject);
        }

	        private void ShowDamageNumber(int amount)
	        {
	            var go = new GameObject("DamageText");
	            go.transform.position = transform.position;
	            go.transform.position += new Vector3(Random.Range(-0.1f, 0.1f), 0.15f, 0f);

	            var textMesh = go.AddComponent<TextMesh>();
	            textMesh.text = amount.ToString();
	            textMesh.fontSize = 32;
	            textMesh.characterSize = 0.15f;
	            textMesh.alignment = TextAlignment.Center;
	            textMesh.anchor = TextAnchor.MiddleCenter;
	            textMesh.color = Color.yellow;

	            if (Camera.main != null)
	            {
	                go.transform.rotation = Camera.main.transform.rotation;
	            }

	            go.AddComponent<FloatingDamageText>();
	        }

        private void CreateHitEffect()
        {
            var effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            effect.name = "HitEffect";
            effect.transform.position = transform.position;
            effect.transform.localScale = Vector3.one * 0.4f;
            effect.GetComponent<Renderer>().material.color = Color.white;
            Destroy(effect.GetComponent<Collider>());
            Destroy(effect, 0.15f);
        }
    }

	    public class FloatingDamageText : MonoBehaviour
	    {
	        private TextMesh _textMesh;
	        private float _duration = 0.8f;
	        private float _elapsed;
	        private Color _startColor;
	        private float _riseSpeed = 1.2f;

	        private void Awake()
	        {
	            _textMesh = GetComponent<TextMesh>();
	            if (_textMesh != null)
	            {
	                _startColor = _textMesh.color;
	            }
	        }

	        private void Update()
	        {
	            _elapsed += Time.deltaTime;

	            transform.position += Vector3.up * (_riseSpeed * Time.deltaTime);

	            if (Camera.main != null)
	            {
	                transform.rotation = Camera.main.transform.rotation;
	            }

	            if (_textMesh != null)
	            {
	                float t = Mathf.Clamp01(_elapsed / _duration);
	                var c = _startColor;
	                c.a = 1f - t;
	                _textMesh.color = c;
	            }

	            if (_elapsed >= _duration)
	            {
	                Destroy(gameObject);
	            }
	        }
	    }
}

