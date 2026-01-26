using UnityEngine;

namespace BoardDefence.Interfaces
{

    public interface IPlaceable
    {
        Vector2Int GridPosition { get; }
        bool IsPlaced { get; }
        
        void Place(Vector2Int position);
        void Remove();
        bool CanBePlacedAt(Vector2Int position);
    }
}

