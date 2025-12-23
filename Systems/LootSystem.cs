using Arkanoid.Core;
using Arkanoid.Enums;

namespace Arkanoid.Systems
{
    // Отвечает за применение эффектов
    public class LootSystem
    {
        public void ApplyLootEffect(LootType type, GameContext context)
        {
            switch (type)
            {
                case LootType.ExtraBall:
                    context.Ball.Velocity = context.Ball.Velocity * 1.2;
                    break;
                case LootType.SpeedUp:
                    context.Paddle.Speed *= 1.1;
                    break;
                    // Легко добавить новые эффекты без изменения класса Loot
            }
        }
    }
}