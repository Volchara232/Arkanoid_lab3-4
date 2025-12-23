using Arkanoid.Core;
using Arkanoid.Interfaces;


namespace Arkanoid.Entities
{
    public class Ball : IGameEntity, ICollidable
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Size { get; set; } = new Vector2(0.8, 0.8);

        public Ball() { }

        public Ball(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        public void Update(double deltaTime, GameContext context)
        {
            // Ball отвечает только за свое перемещение
            Position += Velocity * deltaTime;
        }
    }
}