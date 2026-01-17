namespace BoardDefence.Interfaces
{
    /// <summary>
    /// Interface for any entity that can receive damage
    /// Follows Interface Segregation Principle
    /// </summary>
    public interface IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }
        
        void TakeDamage(int damage);
        void Die();
    }
}

