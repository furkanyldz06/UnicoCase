using System.Collections.Generic;
using UnityEngine;

namespace BoardDefence.Interfaces
{
    /// <summary>
    /// Strategy Pattern interface for different attack behaviors
    /// Allows defence items to have different attack patterns
    /// </summary>
    public interface IAttackStrategy
    {
        /// <summary>
        /// Get all target positions based on the attack strategy
        /// </summary>
        /// <param name="origin">The position of the attacking defence item</param>
        /// <param name="range">Attack range in blocks</param>
        /// <returns>List of grid positions that can be targeted</returns>
        List<Vector2Int> GetTargetPositions(Vector2Int origin, int range);
        
        /// <summary>
        /// Get the direction vectors for this attack strategy
        /// </summary>
        List<Vector2Int> GetAttackDirections();
    }
}

