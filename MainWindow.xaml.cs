using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Arkanoid;
using Arkanoid.Core;
using Arkanoid.State;
using Arkanoid.Enums;
using Arkanoid.Systems;
using Arkanoid.Entities;
using System.Windows.Media.Imaging; // Добавляем для BitmapImage

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
        private const double Scale = 20.0;

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
                RandomSeed = DateTime.Now.Millisecond,
                Lives = 3, // Устанавливаем начальное количество жизней
                GameOver = false
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
                        hitPoints: row + 1,
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
            _gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
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
            if (_game == null) return;

            BallInfo.Text = $"({_game.State.Ball.Position.X:0.0}, {_game.State.Ball.Position.Y:0.0})";
            PaddleInfo.Text = $"({_game.State.Paddle.Position.X:0.0}, {_game.State.Paddle.Position.Y:0.0})";
            BricksInfo.Text = _game.State.Bricks.Count.ToString();
            LootsInfo.Text = _game.State.Loots.Count.ToString();

            // Обновляем счетчик жизней
            LivesInfo.Text = _game.State.Lives.ToString();

            // Показываем/скрываем панель окончания игры
            if (_game.State.GameOver)
            {
                GameOverPanel.Visibility = Visibility.Visible;

                if (_game.State.Bricks.Count == 0)
                {
                    GameOverText.Text = "ПОБЕДА!";
                    GameOverPanel.Background = new SolidColorBrush(Color.FromArgb(255, 0, 100, 0));
                    StatusText.Text = "ПОБЕДА! Все кирпичи разрушены!";
                }
                else if (_game.State.Lives <= 0)
                {
                    GameOverText.Text = "ИГРА ОКОНЧЕНА";
                    GameOverPanel.Background = new SolidColorBrush(Color.FromArgb(255, 139, 0, 0));
                    StatusText.Text = "ИГРА ОКОНЧЕНА! Нажмите R для новой игры.";
                }

                // Деактивируем кнопки при окончании игры
                BtnPause.IsEnabled = false;
                BtnSave.IsEnabled = false;
            }
            else
            {
                GameOverPanel.Visibility = Visibility.Collapsed;
                BtnPause.IsEnabled = true;
                BtnSave.IsEnabled = true;

                // Обновляем статус игры
                if (_isPaused)
                    StatusText.Text = "Игра на паузе";
                else
                    StatusText.Text = "Игра запущена";
            }
        }

        private void DrawGame()
        {
            GameCanvas.Children.Clear();

            if (_game == null) return;

            // Если игра окончена, показываем сообщение на Canvas
            if (_game.State.GameOver)
            {
                var gameOverText = new TextBlock
                {
                    Text = _game.State.Lives <= 0 ? "ИГРА ОКОНЧЕНА" : "ПОБЕДА!",
                    Foreground = _game.State.Lives <= 0 ? Brushes.Red : Brushes.Lime,
                    FontSize = 36,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Центрируем текст
                Canvas.SetLeft(gameOverText, (GameCanvas.ActualWidth / 2) - 120);
                Canvas.SetTop(gameOverText, (GameCanvas.ActualHeight / 2) - 25);
                GameCanvas.Children.Add(gameOverText);

                var restartText = new TextBlock
                {
                    Text = "Нажмите R для новой игры",
                    Foreground = Brushes.White,
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                Canvas.SetLeft(restartText, (GameCanvas.ActualWidth / 2) - 120);
                Canvas.SetTop(restartText, (GameCanvas.ActualHeight / 2) + 25);
                GameCanvas.Children.Add(restartText);

                return; // Не рисуем игру если она окончена
            }

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
                var text = new TextBlock
                {
                    Text = brick.HitPoints.ToString(),
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
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
                GameOverPanel.Visibility = Visibility.Collapsed;
                BtnPause.IsEnabled = true;
                BtnSave.IsEnabled = true;
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

                    // Сбрасываем состояние UI
                    _isPaused = false;
                    BtnPause.Content = "Пауза (P)";
                    GameOverPanel.Visibility = Visibility.Collapsed;
                    BtnPause.IsEnabled = true;
                    BtnSave.IsEnabled = true;
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