using Arkanoid.Core;

namespace Arkanoid.Interfaces
{
    public interface ICollidable
    {
        Vector2 Position { get; set; } // Сеттер нужен для коррекции физикой
        Vector2 Size { get; }
    }
}