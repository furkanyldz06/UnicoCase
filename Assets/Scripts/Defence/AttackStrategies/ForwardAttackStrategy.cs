using System.Collections.Generic;
using BoardDefence.Interfaces;
using UnityEngine;

namespace BoardDefence.Defence.AttackStrategies
{
    /// <summary>
    /// Strategy for forward-only attacks (upward direction)
    /// Implements Strategy Pattern for attack behavior
    /// </summary>
    public class ForwardAttackStrategy : IAttackStrategy
    {
        private static readonly Vector2Int ForwardDirection = new(0, -1); // Up on the board (towards enemies)

        public List<Vector2Int> GetTargetPositions(Vector2Int origin, int range)
        {
            var positions = new List<Vector2Int>();
            
            for (int i = 1; i <= range; i++)
            {
                var targetPos = origin + (ForwardDirection * i);
                positions.Add(targetPos);
            }
            
            return positions;
        }

        public List<Vector2Int> GetAttackDirections()
        {
            return new List<Vector2Int> { ForwardDirection };
        }
    }
}

