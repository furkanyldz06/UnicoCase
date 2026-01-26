using System.Collections.Generic;
using BoardDefence.Interfaces;
using UnityEngine;

namespace BoardDefence.Defence.AttackStrategies
{
    public class AllDirectionAttackStrategy : IAttackStrategy
    {
        private static readonly List<Vector2Int> AllDirections = new()
        {
            new Vector2Int(0, -1),  // Up (forward)
            new Vector2Int(0, 1),   // Down
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(1, 0)    // Right
        };

        public List<Vector2Int> GetTargetPositions(Vector2Int origin, int range)
        {
            var positions = new List<Vector2Int>();
            
            foreach (var direction in AllDirections)
            {
                for (int i = 1; i <= range; i++)
                {
                    var targetPos = origin + (direction * i);
                    positions.Add(targetPos);
                }
            }
            
            return positions;
        }

        public List<Vector2Int> GetAttackDirections()
        {
            return new List<Vector2Int>(AllDirections);
        }
    }
}

