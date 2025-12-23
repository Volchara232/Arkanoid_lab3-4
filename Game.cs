using System.Linq;
using Arkanoid.Core;
using Arkanoid.Entities;
using Arkanoid.Enums;
using Arkanoid.Interfaces;
using Arkanoid.State;
using Arkanoid.Systems;

namespace Arkanoid
{
    public class Game
    {
        public GameState State { get; private set; }

        // Системы
        private readonly ILootDropStrategy _lootDropStrategy;
        private readonly PhysicsSystem _physicsSystem;
        private readonly LootSystem _lootSystem;

        private bool _gameRunning = true;

        public bool IsGameRunning => _gameRunning;

        public Game(GameState initialState)
        {
            State = initialState;
            _lootDropStrategy = new RandomLootDropStrategy(State.RandomSeed);
            _physicsSystem = new PhysicsSystem();
            _lootSystem = new LootSystem();
        }

        public void Update(double deltaTime, InputCommand input)
        {
            if (!_gameRunning) return;
            if (deltaTime > 0.1) deltaTime = 0.1;

            // 1. Создаем контекст
            var context = new GameContext(
                State.Field,
                _lootDropStrategy,
                State.Bricks,
                State.Loots,
                State.Ball,
                State.Paddle);

            // 2. Обработка ввода
            HandleInput(input, deltaTime);

            // 3. Обновление сущностей (перемещение)
            State.Ball.Update(deltaTime, context);
            foreach (var loot in State.Loots)
            {
                loot.Update(deltaTime, context);
            }

            // 4. Физика (столкновения)
            _physicsSystem.UpdatePhysics(context);

            // 5. Логика сбора лута
            ProcessLootCollisions(context);

            // 6. Очистка и проверка условий поражения
            Cleanup();
            CheckGameOver();
        }

        private void HandleInput(InputCommand input, double deltaTime)
        {
            if (input == InputCommand.MoveLeft)
                State.Paddle.MoveLeft(deltaTime, State.Field);
            else if (input == InputCommand.MoveRight)
                State.Paddle.MoveRight(deltaTime, State.Field);
        }

        private void ProcessLootCollisions(GameContext context)
        {
            foreach (var loot in State.Loots.ToList())
            {
                // Простая проверка коллизии (можно вынести в PhysicsSystem, но лут специфичен)
                if (_physicsSystem.CheckAABB(loot, State.Paddle))
                {
                    _lootSystem.ApplyLootEffect(loot.Type, context);
                    State.Loots.Remove(loot);
                }
            }
        }

        private void Cleanup()
        {
            State.Loots.RemoveAll(l => l.Position.Y > State.Field.Height);
        }

        private void CheckGameOver()
        {
            if (State.Ball.Position.Y >= State.Field.Height)
            {
                _gameRunning = false;
            }
        }

        public void ResetGame()
        {
            _gameRunning = true;
            State.Ball.Position = new Vector2(10, 15);
            State.Ball.Velocity = new Vector2(3, -5);
            State.Paddle.Position = new Vector2(8, 28);
            State.Loots.Clear();
        }
    }
}