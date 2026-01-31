using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CrossyGame.Models;
using CrossyGame.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrossyGame
{
    public partial class MainWindow : Window
    {
        private GameState _gameState;
        private DispatcherTimer _timer;
        private const int CellSize = 40;
        private const double DeltaTime = 0.016; // 60 FPS approx

        // Brushes for fallback or overlays
        private static readonly IBrush WarningBrush = Brushes.Yellow;

        public MainWindow()
        {
            InitializeComponent();
            
            // Load Assets
            AssetManager.LoadAssets();

            _gameState = new GameState();
            
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(DeltaTime)
            };
            _timer.Tick += GameLoop;
            _timer.Start();

            // Focus for key events
            this.Opened += (s, e) => this.Focus();
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (_gameState.IsGameOver)
            {
                if (e.Key == Key.R)
                {
                    _gameState.Reset();
                    GameOverText.IsVisible = false;
                }
                return;
            }

            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    _gameState.MovePlayer(0, 1);
                    break;
                case Key.Down:
                case Key.S:
                    _gameState.MovePlayer(0, -1);
                    break;
                case Key.Left:
                case Key.A:
                    _gameState.MovePlayer(-1, 0);
                    break;
                case Key.Right:
                case Key.D:
                    _gameState.MovePlayer(1, 0);
                    break;
            }
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (!_gameState.IsGameOver)
            {
                _gameState.Update(DeltaTime);
                ScoreText.Text = $"Score: {_gameState.Score}";
            }
            else
            {
                GameOverText.IsVisible = true;
            }

            Render();
        }

        private void Render()
        {
            GameCanvas.Children.Clear();

            double centerX = this.Bounds.Width / 2;
            double centerY = this.Bounds.Height / 2;

            // Determine visible range relative to player
            int startY = _gameState.Player.GridY - 8;
            int endY = _gameState.Player.GridY + 12;

            // Draw Lanes
            foreach (var lane in _gameState.Lanes)
            {
                if (lane.YIndex < startY || lane.YIndex > endY) continue;

                double screenY = centerY - ((lane.YIndex - _gameState.Player.GridY) * CellSize);
                
                // Draw Lane Background
                // Use ImageBrush or Tile? For performance, simple Image stretched is fine for now
                // But we need to tile it horizontally to look good.
                // For simplicity: One big rectangle with tiled TextureBrush?
                // Or just one stretched image (might look blurry).
                // Let's draw the generated Bitmap as an Image Control.
                
                var laneImg = new Image
                {
                    Width = GameState.MapWidth * CellSize,
                    Height = CellSize,
                    Source = GetLaneBitmap(lane.Type),
                    Stretch = Stretch.Fill // Stretch our 32x32 tile across the lane? 
                    // To look right, we should use a Tiled Brush, but Avalonia Image doesn't tile easily without DrawingBrush.
                    // We will stretch for now, or it will look like one big tile.
                    // Update: To make it look like tiles, we would need multiple images or a specialized brush.
                    // Given the constraints, Stretched "Grass" looks acceptable as "Turf".
                };
                
                Canvas.SetLeft(laneImg, centerX - (GameState.MapWidth * CellSize / 2));
                Canvas.SetTop(laneImg, screenY);
                GameCanvas.Children.Add(laneImg);

                // Warning Light for Rail
                if (lane.Type == LaneType.Rail && lane.IsTrainComing && lane.TrainTimer < 1.0)
                {
                     if ((int)(DateTime.Now.Millisecond / 200) % 2 == 0)
                     {
                        var warning = new Rectangle { Width = GameState.MapWidth * CellSize, Height = CellSize, Fill = WarningBrush, Opacity = 0.5 };
                        Canvas.SetLeft(warning, centerX - (GameState.MapWidth * CellSize / 2));
                        Canvas.SetTop(warning, screenY);
                        GameCanvas.Children.Add(warning);
                     }
                }

                // Draw Obstacles
                foreach (var obs in lane.Obstacles)
                {
                    if (obs.X < -5 || obs.X > GameState.MapWidth + 5) continue;

                    var obsImg = new Image
                    {
                        Width = obs.Width * CellSize,
                        Height = obs.Height * CellSize,
                        Source = GetObstacleBitmap(obs)
                    };
                    
                    // Flip car if direction is negative?
                    if (obs.Direction < 0 && !obs.IsLog) // Logs are symmetric usually
                    {
                        obsImg.RenderTransform = new ScaleTransform(-1, 1);
                        obsImg.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                    }

                    // Position
                    double obsScreenX = (centerX - (GameState.MapWidth * CellSize / 2)) + (obs.X * CellSize);
                    Canvas.SetLeft(obsImg, obsScreenX);
                    Canvas.SetTop(obsImg, screenY + (CellSize - obsImg.Height)/2); 
                    GameCanvas.Children.Add(obsImg);
                }
            }

            // Draw Player
            var playerImg = new Image
            {
                Width = CellSize * 0.8,
                Height = CellSize * 0.8,
                Source = AssetManager.Assets["chicken"]
            };
            
            double playerScreenX = (centerX - (GameState.MapWidth * CellSize / 2)) + (_gameState.Player.GridX * CellSize) + (CellSize * 0.1);
            double playerScreenY = centerY + (CellSize * 0.1);

            Canvas.SetLeft(playerImg, playerScreenX);
            Canvas.SetTop(playerImg, playerScreenY);
            GameCanvas.Children.Add(playerImg);
        }

        private Bitmap GetLaneBitmap(LaneType type)
        {
            switch (type)
            {
                case LaneType.Grass: return AssetManager.Assets["grass"];
                case LaneType.Road: return AssetManager.Assets["road"];
                case LaneType.Water: return AssetManager.Assets["water"];
                case LaneType.Rail: return AssetManager.Assets["rail"];
                default: return AssetManager.Assets["grass"];
            }
        }
        
        private Bitmap GetObstacleBitmap(Obstacle obs)
        {
             if (obs.IsLog) return AssetManager.Assets["log"];
             // Train logic
             if (obs.Speed > 10) return AssetManager.Assets["train"];
             // Car
             return AssetManager.Assets["car"];
        }
    }
}