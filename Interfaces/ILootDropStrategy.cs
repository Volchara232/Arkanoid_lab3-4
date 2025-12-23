using Arkanoid.Entities;

namespace Arkanoid.Interfaces
{
    public interface ILootDropStrategy
    {
        Loot? TryCreateLoot(Brick brick);
    }
}