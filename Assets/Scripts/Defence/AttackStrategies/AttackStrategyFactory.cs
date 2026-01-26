using BoardDefence.Core.Enums;
using BoardDefence.Interfaces;

namespace BoardDefence.Defence.AttackStrategies
{

    public static class AttackStrategyFactory
    {
        private static readonly ForwardAttackStrategy ForwardStrategy = new();
        private static readonly AllDirectionAttackStrategy AllDirectionStrategy = new();

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

