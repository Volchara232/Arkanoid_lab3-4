using System.Collections.Generic;
using System.Reflection.Metadata;
using Arkanoid.Entities;
using Arkanoid.Interfaces;
using Arkanoid.State;

namespace Arkanoid.Core
{
    // Context передает состояние в методы обновления, чтобы не создавать жестких связей
    public class GameContext
    {
        public GameField Field { get; }
        public ILootDropStrategy LootDropStrategy { get; }
        public List<Brick> Bricks { get; }
        public List<Loot> Loots { get; }
        public Ball Ball { get; }
        public Paddle Paddle { get; }

        public GameContext(GameField field, ILootDropStrategy lootDropStrategy,
            List<Brick> bricks, List<Loot> loots, Ball ball, Paddle paddle)
        {
            Field = field;
            LootDropStrategy = lootDropStrategy;
            Bricks = bricks;
            Loots = loots;
            Ball = ball;
            Paddle = paddle;
        }
    }
}