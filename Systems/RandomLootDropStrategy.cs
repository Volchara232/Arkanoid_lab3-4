using System;
using Arkanoid.Core;
using Arkanoid.Entities;
using Arkanoid.Enums;
using Arkanoid.Interfaces;


namespace Arkanoid.Systems
{
    public class RandomLootDropStrategy : ILootDropStrategy
    {
        private readonly Random _random;

        public RandomLootDropStrategy(int seed)
        {
            _random = new Random(seed);
        }

        public Loot? TryCreateLoot(Brick brick)
        {
            if (brick.LootDropChance <= 0) return null;

            if (_random.NextDouble() <= brick.LootDropChance)
            {
                var pos = new Vector2(
                    brick.Position.X + brick.Size.X / 2.0 - 0.25,
                    brick.Position.Y + brick.Size.Y / 2.0);

                return new Loot(pos, LootType.ExtraBall);
            }
            return null;
        }
    }
}