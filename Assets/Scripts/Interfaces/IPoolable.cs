namespace BoardDefence.Interfaces
{
    /// <summary>
    /// Interface for objects that can be pooled (Object Pool Pattern)
    /// Used for efficient memory management of enemies and projectiles
    /// </summary>
    public interface IPoolable
    {
        bool IsActive { get; }
        
        void OnSpawn();
        void OnDespawn();
        void ResetState();
    }
}

