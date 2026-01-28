using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using LiteGame2D.Engine;
using LiteGame2D.Games;
using System;

namespace LiteGame2D
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private DateTime _lastTime;
        
        // Games
        private IGame _snakeGame;
        private IGame _flappyGame;
        private IGame _bomberGame;
        private IGame _tetrisGame;
        private IGame _pinballGame;
        private IGame _trexGame;

        public MainWindow()
        {
            InitializeComponent();
            
            // Init Input
            Input.Clear();

            // Init Games
            _snakeGame = new SnakeGame();
            _flappyGame = new FlappyBirdGame();
            _bomberGame = new BomberManGame();
            _tetrisGame = new TetrisGame();
            _pinballGame = new PinballGame();
            _trexGame = new TRexGame();

            // Default
            GameScreen.CurrentGame = _snakeGame;
            _snakeGame.Initialize();

            // Game Loop Timer (approx 60 FPS for Logic)
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += Timer_Tick;
            _lastTime = DateTime.Now;
            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var dt = (now - _lastTime).TotalSeconds;
            _lastTime = now;

            // Handle Game Switching
            if (Input.IsKeyDown(Key.D1)) SwitchGame(_snakeGame, "Snake");
            if (Input.IsKeyDown(Key.D2)) SwitchGame(_flappyGame, "Flappy Bird");
            if (Input.IsKeyDown(Key.D3)) SwitchGame(_bomberGame, "Bomber Man");
            if (Input.IsKeyDown(Key.D4)) SwitchGame(_tetrisGame, "Tetris");
            if (Input.IsKeyDown(Key.D5)) SwitchGame(_pinballGame, "Pinball");
            if (Input.IsKeyDown(Key.D6)) SwitchGame(_trexGame, "Jumping T-Rex");

            if (GameScreen.CurrentGame != null)
            {
                GameScreen.CurrentGame.Update(dt);
            }
        }

        private void SwitchGame(IGame game, string title)
        {
            if (GameScreen.CurrentGame != game)
            {
                GameScreen.CurrentGame = game;
                game.Initialize();
                this.Title = $"LiteGame2D - {title} (Press 1-6 to Change)";
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            Input.OnKeyDown(e.Key);
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            Input.OnKeyUp(e.Key);
        }
    }
}