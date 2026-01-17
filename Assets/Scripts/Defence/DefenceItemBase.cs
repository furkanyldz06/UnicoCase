using System.Collections;
using System.Collections.Generic;
using BoardDefence.Core.Enums;
using BoardDefence.Core.Events;
using BoardDefence.Data;
using BoardDefence.Defence.AttackStrategies;
using BoardDefence.Interfaces;
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
            
            foreach (var pos in targetPositions)
            {
                // Check for enemies at this position and damage them
                TryDamageEnemyAt(pos);
            }
            
            GameEvents.RaiseDefenceItemAttacked(_gridPosition, Damage);
        }

        /// <summary>
        /// Try to damage an enemy at the specified position
        /// </summary>
        protected virtual void TryDamageEnemyAt(Vector2Int position)
        {
            // This will be implemented to interact with the enemy system
            var colliders = Physics2D.OverlapPointAll(GetWorldPosition(position));
            
            foreach (var collider in colliders)
            {
                var damageable = collider.GetComponent<IDamageable>();
                if (damageable != null && !damageable.IsDead)
                {
                    damageable.TakeDamage(Damage);
                    break; // Only damage one enemy per position
                }
            }
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
}

