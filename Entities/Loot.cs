using Arkanoid.Core;
using Arkanoid.Enums;
using Arkanoid.Interfaces;


namespace Arkanoid.Entities
{
    public class Loot : IGameEntity, ICollidable
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; } = new Vector2(0.5, 0.5);
        public double FallSpeed { get; set; } = 5.0;
        public LootType Type { get; set; }

        public Loot() { }

        public Loot(Vector2 position, LootType type)
        {
            Position = position;
            Type = type;
        }

        public void Update(double deltaTime, GameContext context)
        {
            Position = new Vector2(Position.X, Position.Y + FallSpeed * deltaTime);
        }
    }
}