using Arkanoid.Core;

namespace Arkanoid.Interfaces
{
    public interface IGameEntity
    {
        void Update(double deltaTime, GameContext context);
    }
}