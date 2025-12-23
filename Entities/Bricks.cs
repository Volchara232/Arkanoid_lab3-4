using Arkanoid.Core;
using Arkanoid.Interfaces;


namespace Arkanoid.Entities
{
    public class Brick : ICollidable
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public int HitPoints { get; set; }
        public double LootDropChance { get; set; }

        public Brick() { }

        public Brick(Vector2 position, Vector2 size, int hitPoints, double lootDropChance)
        {
            Position = position;
            Size = size;
            HitPoints = hitPoints;
            LootDropChance = lootDropChance;
        }

        public bool IsDestroyed => HitPoints <= 0;

        public void TakeDamage()
        {
            if (HitPoints > 0) HitPoints--;
        }
    }
}