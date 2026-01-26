using System.Collections.Generic;
using BoardDefence.Interfaces;
using UnityEngine;

namespace BoardDefence.Defence.AttackStrategies
{

    public class ForwardAttackStrategy : IAttackStrategy
    {
        private static readonly Vector2Int ForwardDirection = new(0, -1);

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

