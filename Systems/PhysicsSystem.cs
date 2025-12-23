using System;
using System.Collections.Generic;
using Arkanoid.Core;
using Arkanoid.Entities;
using Arkanoid.Interfaces;
using Arkanoid.State;

namespace Arkanoid.Systems
{
    public class PhysicsSystem
    {
        public void UpdatePhysics(GameContext context)
        {
            var ball = context.Ball;
            var field = context.Field;

            HandleWallCollisions(ball, field);
            HandlePaddleCollision(ball, context.Paddle);
            HandleBrickCollisions(ball, context.Bricks, context);
        }

        private void HandleWallCollisions(Ball ball, GameField field)
        {
            bool collided = false;
            if (ball.Position.X <= 0)
            {
                ball.Position = new Vector2(0, ball.Position.Y);
                ball.Velocity = new Vector2(Math.Abs(ball.Velocity.X), ball.Velocity.Y);
                collided = true;
            }
            else if (ball.Position.X + ball.Size.X >= field.Width)
            {
                ball.Position = new Vector2(field.Width - ball.Size.X, ball.Position.Y);
                ball.Velocity = new Vector2(-Math.Abs(ball.Velocity.X), ball.Velocity.Y);
                collided = true;
            }

            if (ball.Position.Y <= 0)
            {
                ball.Position = new Vector2(ball.Position.X, 0);
                ball.Velocity = new Vector2(ball.Velocity.X, Math.Abs(ball.Velocity.Y));
                collided = true;
            }

            if (collided) ball.Position += ball.Velocity * 0.01;
        }

        private void HandlePaddleCollision(Ball ball, Paddle paddle)
        {
            if (!CheckAABB(ball, paddle)) return;

            // Расчет угла отскока
            double paddleCenter = paddle.Position.X + paddle.Size.X / 2;
            double ballCenter = ball.Position.X + ball.Size.X / 2;
            double relativeIntersect = (ballCenter - paddleCenter) / (paddle.Size.X / 2);
            double bounceAngle = relativeIntersect * Math.PI / 3; // до 60 градусов

            double speed = ball.Velocity.Length();
            ball.Velocity = new Vector2(
                Math.Sin(bounceAngle) * speed,
                -Math.Abs(Math.Cos(bounceAngle) * speed)
            );

            // Выталкиваем мяч
            ball.Position = new Vector2(ball.Position.X, paddle.Position.Y - ball.Size.Y - 0.01);
        }

        private void HandleBrickCollisions(Ball ball, List<Brick> bricks, GameContext context)
        {
            foreach (var brick in bricks)
            {
                if (brick.IsDestroyed) continue;

                if (CheckAABB(ball, brick))
                {
                    brick.TakeDamage();

                    // Логика дропа лута перенесена в LootSystem или вызывается тут, 
                    // но создание лута лучше делать в Game.cs на основе события или флага.
                    // Для простоты оставим прямую модификацию списка лута:
                    if (brick.IsDestroyed)
                    {
                        var loot = context.LootDropStrategy.TryCreateLoot(brick);
                        if (loot != null) context.Loots.Add(loot);
                    }

                    ResolveBrickBounce(ball, brick);
                    break; // Обрабатываем только одно столкновение за кадр для простоты
                }
            }
            bricks.RemoveAll(b => b.IsDestroyed);
        }

        private void ResolveBrickBounce(Ball ball, Brick brick)
        {
            // Упрощенная логика отражения
            double overlapLeft = (ball.Position.X + ball.Size.X) - brick.Position.X;
            double overlapRight = (brick.Position.X + brick.Size.X) - ball.Position.X;
            double overlapTop = (ball.Position.Y + ball.Size.Y) - brick.Position.Y;
            double overlapBottom = (brick.Position.Y + brick.Size.Y) - ball.Position.Y;

            double minOverlap = Math.Min(Math.Min(overlapLeft, overlapRight), Math.Min(overlapTop, overlapBottom));

            if (minOverlap == overlapLeft || minOverlap == overlapRight)
            {
                ball.Velocity = new Vector2(-ball.Velocity.X, ball.Velocity.Y);
                // Коррекция позиции
                ball.Position = new Vector2(minOverlap == overlapLeft ? brick.Position.X - ball.Size.X : brick.Position.X + brick.Size.X, ball.Position.Y);
            }
            else
            {
                ball.Velocity = new Vector2(ball.Velocity.X, -ball.Velocity.Y);
                ball.Position = new Vector2(ball.Position.X, minOverlap == overlapTop ? brick.Position.Y - ball.Size.Y : brick.Position.Y + brick.Size.Y);
            }
        }

        public bool CheckAABB(ICollidable a, ICollidable b)
        {
            return a.Position.X < b.Position.X + b.Size.X &&
                   a.Position.X + a.Size.X > b.Position.X &&
                   a.Position.Y < b.Position.Y + b.Size.Y &&
                   a.Position.Y + a.Size.Y > b.Position.Y;
        }
    }
}