using System.Collections.Generic;
using UnityEngine;

namespace BoardDefence.Interfaces
{

    public interface IAttackStrategy
    {
        List<Vector2Int> GetTargetPositions(Vector2Int origin, int range);   
        List<Vector2Int> GetAttackDirections();
    }
}

