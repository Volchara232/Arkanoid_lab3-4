using System.Linq;
using Arkanoid.Core;
using Arkanoid.Entities;
using Arkanoid.Enums;
using Arkanoid.Interfaces;
using Arkanoid.State;
using Arkanoid.Systems;
using System; // Добавляем using для Math

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
        private bool _isBallResetting = false; // Флаг для перезапуска мяча
        private double _resetTimer = 0; // Таймер для задержки перезапуска
        private const double RESET_DELAY = 1.0; // Задержка в секундах перед перезапуском

        public bool IsGameRunning => _gameRunning && !State.GameOver;

        public bool IsBallResetting => _isBallResetting;
        public double ResetTimer => _resetTimer;

        public Game(GameState initialState)
        {
            State = initialState;
            _lootDropStrategy = new RandomLootDropStrategy(State.RandomSeed);
            _physicsSystem = new PhysicsSystem();
            _lootSystem = new LootSystem();
        }

        public void Update(double deltaTime, InputCommand input)
        {
            if (!_gameRunning || State.GameOver) return;
            if (deltaTime > 0.1) deltaTime = 0.1;

            // Если мяч перезапускается
            if (_isBallResetting)
            {
                _resetTimer -= deltaTime;
                if (_resetTimer <= 0)
                {
                    ResetBallPosition();
                    _isBallResetting = false;
                }
                return; // Не обновляем игру пока мяч перезапускается
            }

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

            // 5. Проверка вылета мяча
            CheckBallOutOfBounds();

            // 6. Логика сбора лута
            ProcessLootCollisions(context);

            // 7. Очистка
            Cleanup();

            // 8. Проверка победы/поражения
            CheckWinCondition();
            CheckGameOver();
        }

        private void HandleInput(InputCommand input, double deltaTime)
        {
            if (input == InputCommand.MoveLeft)
                State.Paddle.MoveLeft(deltaTime, State.Field);
            else if (input == InputCommand.MoveRight)
                State.Paddle.MoveRight(deltaTime, State.Field);
        }

        private void CheckBallOutOfBounds()
        {
            // Проверяем, улетел ли мяч за нижнюю границу
            if (State.Ball.Position.Y >= State.Field.Height)
            {
                LoseLife();
            }
        }

        private void LoseLife()
        {
            State.Lives--;

            if (State.Lives > 0)
            {
                // Начинаем перезапуск мяча с задержкой
                _isBallResetting = true;
                _resetTimer = RESET_DELAY;

                // Сохраняем текущую позицию платформы для старта мяча
                // Мяч будет запущен в следующем кадре
            }
            else
            {
                State.GameOver = true;
                State.Lives = 0; // Гарантируем, что жизни не станут отрицательными
            }
        }

        private void ResetBallPosition()
        {
            // Размещаем мяч над платформой
            double paddleCenter = State.Paddle.Position.X + State.Paddle.Size.X / 2;
            State.Ball.Position = new Vector2(
                paddleCenter - State.Ball.Size.X / 2,
                State.Paddle.Position.Y - State.Ball.Size.Y - 0.1
            );

            // Задаем начальную скорость
            State.Ball.Velocity = new Vector2(0, -5); // Мяч стартует вертикально вверх
        }

        private void ProcessLootCollisions(GameContext context)
        {
            foreach (var loot in State.Loots.ToList())
            {
                if (_physicsSystem.CheckAABB(loot, State.Paddle))
                {
                    _lootSystem.ApplyLootEffect(loot.Type, context);

                    // Добавляем жизнь при сборе лутов (опционально)
                    if (loot.Type == LootType.ExtraBall)
                    {
                        AddLife();
                    }

                    State.Loots.Remove(loot);
                }
            }
        }

        private void AddLife()
        {
            if (State.Lives < 5) // Максимум 5 жизней
            {
                State.Lives++;
            }
        }

        private void Cleanup()
        {
            State.Loots.RemoveAll(l => l.Position.Y > State.Field.Height);
        }

        private void CheckWinCondition()
        {
            if (State.Bricks.Count == 0)
            {
                State.GameOver = true;
            }
        }

        private void CheckGameOver()
        {
            if (State.Lives <= 0)
            {
                State.GameOver = true;
                State.Lives = 0;
            }
        }

        public void ResetGame()
        {
            State.GameOver = false;
            State.Lives = 3;
            _gameRunning = true;
            _isBallResetting = false;
            ResetBallPosition();
            State.Paddle.Position = new Vector2(8, 28);
            State.Loots.Clear();

            // Восстанавливаем кирпичи
            if (State.Bricks.Count == 0)
            {
                // Создаем начальные кирпичи
                var rows = 5;
                var cols = 8;
                var brickWidth = 2.0;
                var brickHeight = 1.0;
                var startX = 1.0;
                var startY = 1.0;

                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        var position = new Vector2(
                            startX + col * (brickWidth + 0.2),
                            startY + row * (brickHeight + 0.2));

                        var brick = new Brick(
                            position,
                            new Vector2(brickWidth, brickHeight),
                            hitPoints: row + 1,
                            lootDropChance: 0.2
                        );

                        State.Bricks.Add(brick);
                    }
                }
            }
        }
    }
}