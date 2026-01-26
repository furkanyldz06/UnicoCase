namespace BoardDefence.Interfaces
{

    public interface IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }
        
        void TakeDamage(int damage);
        void Die();
    }
}

