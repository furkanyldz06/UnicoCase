using BoardDefence.Core.Enums;
using BoardDefence.Interfaces;

namespace BoardDefence.Defence.AttackStrategies
{
    /// <summary>
    /// Factory for creating attack strategies based on AttackDirection enum
    /// Implements Factory Pattern
    /// </summary>
    public static class AttackStrategyFactory
    {
        private static readonly ForwardAttackStrategy ForwardStrategy = new();
        private static readonly AllDirectionAttackStrategy AllDirectionStrategy = new();

        /// <summary>
        /// Get the appropriate attack strategy based on direction type
        /// Uses cached instances for performance (Flyweight-like pattern)
        /// </summary>
        public static IAttackStrategy GetStrategy(AttackDirection direction)
        {
            return direction switch
            {
                AttackDirection.Forward => ForwardStrategy,
                AttackDirection.All => AllDirectionStrategy,
                _ => ForwardStrategy
            };
        }
    }
}

