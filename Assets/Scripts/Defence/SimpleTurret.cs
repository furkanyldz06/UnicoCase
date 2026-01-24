using System.Collections;
using System.Collections.Generic;
using BoardDefence.Core.Enums;
using BoardDefence.Interfaces;
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
        private bool _isAttacking;
        private Coroutine _attackCoroutine;

        public void Initialize(DefenceItemType type, Vector2Int gridPos)
        {
            _type = type;
            _gridPosition = gridPos;

            // Set stats based on type
            // Range = kaç hücre ilerisine ateş edebilir (1 hücre = ~1 birim)
            switch (type)
            {
                case DefenceItemType.Type1:
                    _damage = 3;
                    _range = 3f;      // 3 hücre menzil
                    _attackInterval = 0.8f;
                    break;
                case DefenceItemType.Type2:
                    _damage = 5;
                    _range = 2f;      // 2 hücre menzil
                    _attackInterval = 1.2f;
                    break;
                case DefenceItemType.Type3:
                    _damage = 10;
                    _range = 1.5f;    // 1.5 hücre menzil
                    _attackInterval = 1.5f;
                    break;
            }

            // Auto start attacking
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
            
            // Find all enemies
            var allObjects = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            
            foreach (var obj in allObjects)
            {
                if (obj.name.Contains("Enemy") || obj.name.Contains("enemy"))
                {
                    float dist = Vector3.Distance(transform.position, obj.position);
                    if (dist <= _range && dist < nearestDist)
                    {
                        nearest = obj.gameObject;
                        nearestDist = dist;
                    }
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

