using System;

namespace Arkanoid.Core
{
    public struct Vector2
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0, 0);

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 v, double scalar) => new Vector2(v.X * scalar, v.Y * scalar);

        public double Length() => Math.Sqrt(X * X + Y * Y);

        // Нормализация вектора
        public Vector2 Normalized()
        {
            var len = Length();
            return len == 0 ? Zero : new Vector2(X / len, Y / len);
        }
    }
}