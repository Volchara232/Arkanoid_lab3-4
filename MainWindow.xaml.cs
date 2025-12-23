using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Arkanoid;
using Arkanoid.Core;           // Для Game, Vector2
using Arkanoid.State;     // Для GameState, Ball, Paddle, Brick, Loot
using Arkanoid.Enums;     // Для InputCommand
using Arkanoid.Systems;
using Arkanoid.Entities;

namespace ArkanoidWPF
{
    public partial class MainWindow : Window
    {
        private Game _game;
        private DispatcherTimer _gameTimer;
        private DateTime _lastUpdateTime;
        private bool _isPaused = false;
        private bool _leftKeyPressed = false;
        private bool _rightKeyPressed = false;
        private const double Scale = 20.0; // Масштабирование для отрисовки

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
            InitializeTimer();
        }

        private void InitializeGame()
        {
            try
            {
                if (System.IO.File.Exists("game_state.json"))
                {
                    var state = GameStateSerializer.Load("game_state.json");
                    _game = new Game(state);
                    StatusText.Text = "Игра загружена";
                }
                else
                {
                    var state = CreateInitialState();
                    _game = new Game(state);
                    StatusText.Text = "Новая игра";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки игры: {ex.Message}");
                var state = CreateInitialState();
                _game = new Game(state);
                StatusText.Text = "Новая игра (ошибка загрузки)";
            }

            _lastUpdateTime = DateTime.Now;
        }

        private GameState CreateInitialState()
        {
            var state = new GameState
            {
                Field = new GameField(20, 30),
                Ball = new Ball(new Vector2(10, 15), new Vector2(3, -5)),
                Paddle = new Paddle(new Vector2(8, 28), new Vector2(4, 1)),
                RandomSeed = DateTime.Now.Millisecond
            };

            // Создаем кирпичи
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
                        hitPoints: row + 1, // Разное количество HP для разнообразия
                        lootDropChance: 0.2
                    );

                    state.Bricks.Add(brick);
                }
            }

            return state;
        }

        private void InitializeTimer()
        {
            _gameTimer = new DispatcherTimer();
           // _gameTimer.Interval = TimeSpan.FromMilliseconds(1); // ~60 FPS
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (_isPaused || _game == null)
                return;

            var now = DateTime.Now;
            var deltaTime = (now - _lastUpdateTime).TotalSeconds;
            _lastUpdateTime = now;

            // Обработка ввода
            InputCommand input = InputCommand.None;
            if (_leftKeyPressed) input = InputCommand.MoveLeft;
            else if (_rightKeyPressed) input = InputCommand.MoveRight;

            // Обновление игры
            _game.Update(deltaTime, input);

            // Обновление интерфейса
            UpdateGameInfo();
            DrawGame();
        }

        private void UpdateGameInfo()
        {
            BallInfo.Text = $"({_game.State.Ball.Position.X:0.0}, {_game.State.Ball.Position.Y:0.0})";
            PaddleInfo.Text = $"({_game.State.Paddle.Position.X:0.0}, {_game.State.Paddle.Position.Y:0.0})";
            BricksInfo.Text = _game.State.Bricks.Count.ToString();
            LootsInfo.Text = _game.State.Loots.Count.ToString();
        }

        private void DrawGame()
        {
            GameCanvas.Children.Clear();

            // Рисуем границы поля
            var border = new Rectangle
            {
                Width = _game.State.Field.Width * Scale,
                Height = _game.State.Field.Height * Scale,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };
            Canvas.SetLeft(border, 0);
            Canvas.SetTop(border, 0);
            GameCanvas.Children.Add(border);

            // Рисуем кирпичи
            foreach (var brick in _game.State.Bricks)
            {
                var rect = new Rectangle
                {
                    Width = brick.Size.X * Scale,
                    Height = brick.Size.Y * Scale,
                    Fill = GetBrickColor(brick.HitPoints),
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rect, brick.Position.X * Scale);
                Canvas.SetTop(rect, brick.Position.Y * Scale);
                GameCanvas.Children.Add(rect);

                // Показываем HP на кирпиче
                var text = new System.Windows.Controls.TextBlock
                {
                    Text = brick.HitPoints.ToString(),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                Canvas.SetLeft(text, brick.Position.X * Scale + brick.Size.X * Scale / 2 - 8);
                Canvas.SetTop(text, brick.Position.Y * Scale + brick.Size.Y * Scale / 2 - 8);
                GameCanvas.Children.Add(text);
            }

            // Рисуем мяч
            var ball = new Ellipse
            {
                Width = _game.State.Ball.Size.X * Scale,
                Height = _game.State.Ball.Size.Y * Scale,
                Fill = Brushes.Yellow,
                Stroke = Brushes.Orange,
                StrokeThickness = 2
            };
            Canvas.SetLeft(ball, _game.State.Ball.Position.X * Scale);
            Canvas.SetTop(ball, _game.State.Ball.Position.Y * Scale);
            GameCanvas.Children.Add(ball);

            // Рисуем платформу
            var paddle = new Rectangle
            {
                Width = _game.State.Paddle.Size.X * Scale,
                Height = _game.State.Paddle.Size.Y * Scale,
                Fill = Brushes.Cyan,
                Stroke = Brushes.Blue,
                StrokeThickness = 2,
                RadiusX = 5,
                RadiusY = 5
            };
            Canvas.SetLeft(paddle, _game.State.Paddle.Position.X * Scale);
            Canvas.SetTop(paddle, _game.State.Paddle.Position.Y * Scale);
            GameCanvas.Children.Add(paddle);

            // Рисуем бонусы
            foreach (var loot in _game.State.Loots)
            {
                var lootShape = new Ellipse
                {
                    Width = loot.Size.X * Scale,
                    Height = loot.Size.Y * Scale,
                    Fill = Brushes.Green,
                    Stroke = Brushes.Lime,
                    StrokeThickness = 2
                };
                Canvas.SetLeft(lootShape, loot.Position.X * Scale);
                Canvas.SetTop(lootShape, loot.Position.Y * Scale);
                GameCanvas.Children.Add(lootShape);
            }
        }

        private Brush GetBrickColor(int hitPoints)
        {
            return hitPoints switch
            {
                1 => Brushes.Red,
                2 => Brushes.Orange,
                3 => Brushes.Yellow,
                4 => Brushes.Green,
                5 => Brushes.Blue,
                _ => Brushes.Purple
            };
        }

        // Обработчики событий клавиатуры
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    _leftKeyPressed = true;
                    break;
                case Key.D:
                case Key.Right:
                    _rightKeyPressed = true;
                    break;
                case Key.S:
                    SaveGame();
                    break;
                case Key.R:
                    StartNewGame();
                    break;
                case Key.P:
                    TogglePause();
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    _leftKeyPressed = false;
                    break;
                case Key.D:
                case Key.Right:
                    _rightKeyPressed = false;
                    break;
            }
        }

        // Кнопки управления
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            TogglePause();
        }

        private void BtnNewGame_Click(object sender, RoutedEventArgs e)
        {
            StartNewGame();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveGame();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadGame();
        }

        // Методы управления игрой
        private void TogglePause()
        {
            _isPaused = !_isPaused;
            BtnPause.Content = _isPaused ? "Продолжить (P)" : "Пауза (P)";
            StatusText.Text = _isPaused ? "Игра на паузе" : "Игра запущена";
        }

        private void StartNewGame()
        {
            var result = MessageBox.Show("Начать новую игру? Несохранённый прогресс будет потерян.",
                "Новая игра", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var state = CreateInitialState();
                _game = new Game(state);
                _isPaused = false;
                BtnPause.Content = "Пауза (P)";
                StatusText.Text = "Новая игра";
            }
        }

        private void SaveGame()
        {
            try
            {
                GameStateSerializer.Save(_game.State, "game_state.json");
                StatusText.Text = "Игра сохранена";
                MessageBox.Show("Игра успешно сохранена!", "Сохранение",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadGame()
        {
            try
            {
                if (System.IO.File.Exists("game_state.json"))
                {
                    var state = GameStateSerializer.Load("game_state.json");
                    _game = new Game(state);
                    StatusText.Text = "Игра загружена";
                    MessageBox.Show("Игра успешно загружена!", "Загрузка",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Файл сохранения не найден.", "Загрузка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Сохранить игру перед выходом?", "Выход",
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                SaveGame();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}