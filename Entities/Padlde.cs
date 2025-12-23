using System;
using Arkanoid.Core;
using Arkanoid.Interfaces;
using Arkanoid.State;


namespace Arkanoid.Entities
{
    public class Paddle : ICollidable
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public double Speed { get; set; } = 10.0;

        public Paddle() { }

        public Paddle(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        // Логику перемещения оставляем тут, так как это специфичное поведение сущности
        public void MoveLeft(double deltaTime, GameField field)
        {
            Position = new Vector2(Math.Max(0, Position.X - Speed * deltaTime), Position.Y);
        }

        public void MoveRight(double deltaTime, GameField field)
        {
            Position = new Vector2(Math.Min(field.Width - Size.X, Position.X + Speed * deltaTime), Position.Y);
        }
    }
}