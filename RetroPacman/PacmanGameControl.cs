using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RetroPacman
{
    public enum GameState
    {
        Menu,
        Playing,
        GameOver,
        Win
    }

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public enum TileType
    {
        Empty = 0,
        Wall = 1,
        Pellet = 2,
        PowerPellet = 3
    }

    public class PacmanGameControl : Control
    {
        // Game Settings
        private const int TileSize = 20;
        private const int MapWidth = 28;
        private const int MapHeight = 31;
        private const int PowerModeDurationTicks = 50; // 50 * 200ms = 10 seconds approx (depends on timer)

        // Game State
        private GameState _currentState = GameState.Menu;
        private DispatcherTimer _gameTimer;
        private int _score = 0;
        private int _level = 1;
        private int _lives = 3;
        
        // Power Mode
        private int _powerModeTimeLeft = 0;
        private int _ghostsEatenInPowerMode = 0;

        // Animation
        private double _mouthAngle = 45;
        private bool _mouthClosing = true;

        // Map
        private TileType[,] _map = new TileType[MapWidth, MapHeight];
        
        // Entities
        private Point _pacmanPos;
        private Direction _pacmanDir;
        private Direction _nextDir; // For smoother turning
        
        private List<Ghost> _ghosts = new List<Ghost>();
        
        // Graphics - Brushes
        private readonly IBrush _wallBrush = new SolidColorBrush(Color.Parse("#1919A6")); // Classic Blue
        private readonly IBrush _pelletBrush = new SolidColorBrush(Color.Parse("#FFB8AE"));
        private readonly IBrush _powerPelletBrush = new SolidColorBrush(Color.Parse("#FFB8AE"));
        private readonly IBrush _pacmanBrush = Brushes.Yellow;
        private readonly IBrush _ghostFrightenedBrush = Brushes.Blue;
        private readonly IBrush _ghostFrightenedEndingBrush = Brushes.White;

        public PacmanGameControl()
        {
            // Enable keyboard focus
            Focusable = true;
            
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(150); // Base Speed
            _gameTimer.Tick += GameLoop;
            
            InitGame();
        }

        private void InitGame()
        {
            _score = 0;
            _level = 1;
            _lives = 3;
            LoadLevel();
        }

        private void LoadLevel()
        {
            GenerateMap();

            _pacmanPos = new Point(14, 23); // Start pos
            _pacmanDir = Direction.None;
            _nextDir = Direction.None;
            _powerModeTimeLeft = 0;

            _ghosts = new List<Ghost>();
            // Add ghosts based on level
            int ghostCount = Math.Min(4 + (_level - 1), 8);
            for (int i = 0; i < ghostCount; i++)
            {
                // Assign different colors
                IBrush color = Brushes.Red;
                if (i % 4 == 1) color = Brushes.Cyan;
                else if (i % 4 == 2) color = Brushes.Pink;
                else if (i % 4 == 3) color = Brushes.Orange;

                _ghosts.Add(new Ghost { 
                    Pos = new Point(14, 11), 
                    Dir = Direction.Up, 
                    NormalColor = color,
                    IsDead = false
                });
            }

            // Adjust speed slightly based on level
            double speedMs = Math.Max(80, 150 - (_level * 5));
            _gameTimer.Interval = TimeSpan.FromMilliseconds(speedMs);
        }

        private void GenerateMap()
        {
            _map = new TileType[MapWidth, MapHeight];

            // Initialize borders
            for(int x=0; x<MapWidth; x++)
            {
                for(int y=0; y<MapHeight; y++)
                {
                    if (x == 0 || x == MapWidth - 1 || y == 0 || y == MapHeight - 1)
                        _map[x, y] = TileType.Wall;
                    else
                        _map[x, y] = TileType.Pellet; // Default to pellet
                }
            }
            
            // Add Walls (Simplified Maze)
            AddWallRect(2, 2, 4, 3);
            AddWallRect(7, 2, 5, 3);
            AddWallRect(16, 2, 5, 3);
            AddWallRect(22, 2, 4, 3);
            
            AddWallRect(2, 6, 4, 2);
            AddWallRect(7, 6, 2, 8);
            AddWallRect(10, 6, 8, 2);
            AddWallRect(19, 6, 2, 8);
            AddWallRect(22, 6, 4, 2);

            // T-Shapes and other blocks
            AddWallRect(2, 25, 10, 2);
            AddWallRect(16, 25, 10, 2);
            AddWallRect(14, 18, 2, 5);

            // Ghost house
            AddWallRect(10, 12, 8, 5); 
            // Clear inside ghost house
            for(int x=11; x<=16; x++)
                for(int y=13; y<=15; y++)
                    _map[x, y] = TileType.Empty;
            
            // Remove pellets from walls (integrity check)
            for (int x = 0; x < MapWidth; x++)
                for (int y = 0; y < MapHeight; y++)
                    if (_map[x, y] == TileType.Wall) _map[x, y] = TileType.Wall;

            // Clear Pacman spawn
            _map[14, 23] = TileType.Empty;
            
            // Add Power Pellets (Coins) in corners
            if (_map[1, 1] != TileType.Wall) _map[1, 1] = TileType.PowerPellet;
            if (_map[MapWidth-2, 1] != TileType.Wall) _map[MapWidth-2, 1] = TileType.PowerPellet;
            if (_map[1, MapHeight-2] != TileType.Wall) _map[1, MapHeight-2] = TileType.PowerPellet;
            if (_map[MapWidth-2, MapHeight-2] != TileType.Wall) _map[MapWidth-2, MapHeight-2] = TileType.PowerPellet;
        }

        private void AddWallRect(int x, int y, int w, int h)
        {
            for(int i=x; i<x+w; i++)
                for(int j=y; j<y+h; j++)
                    if(i < MapWidth && j < MapHeight)
                        _map[i, j] = TileType.Wall;
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (_currentState != GameState.Playing) return;

            UpdateGameLogic();
            InvalidateVisual(); // Trigger Render
        }

        private void UpdateGameLogic()
        {
            // Update Power Mode
            if (_powerModeTimeLeft > 0)
            {
                _powerModeTimeLeft--;
            }

            // Animate Mouth
            if (_mouthClosing)
            {
                _mouthAngle -= 10;
                if (_mouthAngle <= 5) _mouthClosing = false;
            }
            else
            {
                _mouthAngle += 10;
                if (_mouthAngle >= 45) _mouthClosing = true;
            }

            MovePacman();
            MoveGhosts();
            CheckCollisions();
            CheckLevelComplete();
        }

        private void MovePacman()
        {
            // Try to change direction if queued
            if (_nextDir != Direction.None && CanMove(_pacmanPos, _nextDir))
            {
                _pacmanDir = _nextDir;
                _nextDir = Direction.None;
            }

            if (CanMove(_pacmanPos, _pacmanDir))
            {
                _pacmanPos = GetNextPos(_pacmanPos, _pacmanDir);
                
                // Eat item
                int x = (int)_pacmanPos.X;
                int y = (int)_pacmanPos.Y;
                
                if (_map[x, y] == TileType.Pellet)
                {
                    _map[x, y] = TileType.Empty;
                    _score += 10;
                }
                else if (_map[x, y] == TileType.PowerPellet)
                {
                    _map[x, y] = TileType.Empty;
                    _score += 50;
                    ActivatePowerMode();
                }
            }
        }

        private void ActivatePowerMode()
        {
            _powerModeTimeLeft = PowerModeDurationTicks;
            _ghostsEatenInPowerMode = 0;
            // Force ghosts to reverse direction immediately (classic mechanic)
            foreach(var ghost in _ghosts)
            {
                if (!ghost.IsDead)
                    ghost.Dir = GetOpposite(ghost.Dir);
            }
        }

        private void MoveGhosts()
        {
            Random rnd = new Random();
            foreach (var ghost in _ghosts)
            {
                if (ghost.IsDead)
                {
                    // Return to spawn logic (simplified: move towards center)
                    // Or actually, let's just respawn them after a delay or move them towards house.
                    // Simplified: if Dead, move towards house. if at house, revive.
                    if (ghost.Pos.X == 14 && ghost.Pos.Y == 11)
                    {
                        ghost.IsDead = false;
                    }
                    else
                    {
                         // Very basic homing to 14,11
                         Direction targetDir = Direction.None;
                         if (ghost.Pos.X < 14) targetDir = Direction.Right;
                         else if (ghost.Pos.X > 14) targetDir = Direction.Left;
                         else if (ghost.Pos.Y < 11) targetDir = Direction.Down;
                         else if (ghost.Pos.Y > 11) targetDir = Direction.Up;

                         if (CanMove(ghost.Pos, targetDir)) ghost.Dir = targetDir;
                         else ghost.Dir = GetRandomValidDir(ghost.Pos, ghost.Dir, rnd);
                    }
                }
                else
                {
                    // Normal AI
                    bool isFrightened = _powerModeTimeLeft > 0;
                    
                    Direction bestDir = ghost.Dir;

                    if (isFrightened)
                    {
                        // Run away from Pacman
                        // Simplified: Pick random direction that is NOT towards Pacman roughly
                         bestDir = GetRandomValidDir(ghost.Pos, ghost.Dir, rnd);
                    }
                    else
                    {
                        // Chase Pacman (Simple Tracker)
                        if (rnd.NextDouble() < 0.5 + (_level * 0.05)) 
                        {
                            if (_pacmanPos.X < ghost.Pos.X && CanMove(ghost.Pos, Direction.Left)) bestDir = Direction.Left;
                            else if (_pacmanPos.X > ghost.Pos.X && CanMove(ghost.Pos, Direction.Right)) bestDir = Direction.Right;
                            else if (_pacmanPos.Y < ghost.Pos.Y && CanMove(ghost.Pos, Direction.Up)) bestDir = Direction.Up;
                            else if (_pacmanPos.Y > ghost.Pos.Y && CanMove(ghost.Pos, Direction.Down)) bestDir = Direction.Down;
                        }
                    }

                    // If blocked or current choice invalid, pick random valid
                    if (!CanMove(ghost.Pos, bestDir) || (isFrightened && rnd.NextDouble() < 0.3))
                    {
                        bestDir = GetRandomValidDir(ghost.Pos, ghost.Dir, rnd);
                    }
                    
                    ghost.Dir = bestDir;
                }

                if (CanMove(ghost.Pos, ghost.Dir))
                {
                    ghost.Pos = GetNextPos(ghost.Pos, ghost.Dir);
                }
            }
        }

        private Direction GetRandomValidDir(Point pos, Direction currentDir, Random rnd)
        {
            var validDirs = new List<Direction>();
            // Don't reverse immediately unless stuck, usually ghosts don't reverse
            Direction opposite = GetOpposite(currentDir);

            if (CanMove(pos, Direction.Up) && Direction.Up != opposite) validDirs.Add(Direction.Up);
            if (CanMove(pos, Direction.Down) && Direction.Down != opposite) validDirs.Add(Direction.Down);
            if (CanMove(pos, Direction.Left) && Direction.Left != opposite) validDirs.Add(Direction.Left);
            if (CanMove(pos, Direction.Right) && Direction.Right != opposite) validDirs.Add(Direction.Right);

            if (validDirs.Count == 0) return opposite; // Dead end
            return validDirs[rnd.Next(validDirs.Count)];
        }

        private void CheckCollisions()
        {
            foreach (var ghost in _ghosts)
            {
                if (Math.Abs(ghost.Pos.X - _pacmanPos.X) < 0.8 && Math.Abs(ghost.Pos.Y - _pacmanPos.Y) < 0.8)
                {
                    if (_powerModeTimeLeft > 0 && !ghost.IsDead)
                    {
                        // Eat Ghost
                        ghost.IsDead = true;
                        _ghostsEatenInPowerMode++;
                        _score += (int)(200 * Math.Pow(2, _ghostsEatenInPowerMode - 1));
                    }
                    else if (!ghost.IsDead)
                    {
                        // Pacman Dies
                        _lives--;
                        if (_lives <= 0)
                        {
                            _currentState = GameState.GameOver;
                            _gameTimer.Stop();
                        }
                        else
                        {
                            ResetPositions();
                        }
                        break;
                    }
                }
            }
        }

        private void ResetPositions()
        {
            _pacmanPos = new Point(14, 23);
            _pacmanDir = Direction.None;
            _nextDir = Direction.None;
            _powerModeTimeLeft = 0;
            
            foreach(var g in _ghosts) 
            {
                g.Pos = new Point(14, 11);
                g.Dir = Direction.Up;
                g.IsDead = false;
            }
        }

        private void CheckLevelComplete()
        {
            bool pelletsLeft = false;
            for (int x = 0; x < MapWidth; x++)
                for (int y = 0; y < MapHeight; y++)
                    if (_map[x, y] == TileType.Pellet || _map[x, y] == TileType.PowerPellet) 
                        pelletsLeft = true;

            if (!pelletsLeft)
            {
                _level++;
                LoadLevel(); 
            }
        }

        private bool CanMove(Point pos, Direction dir)
        {
            Point next = GetNextPos(pos, dir);
            int x = (int)next.X;
            int y = (int)next.Y;

            if (x < 0 || x >= MapWidth || y < 0 || y >= MapHeight) return false;
            return _map[x, y] != TileType.Wall;
        }

        private Point GetNextPos(Point pos, Direction dir)
        {
            // Simple warp tunnel
            if (pos.X <= 0 && dir == Direction.Left) return new Point(MapWidth - 1, pos.Y);
            if (pos.X >= MapWidth - 1 && dir == Direction.Right) return new Point(0, pos.Y);

            return dir switch
            {
                Direction.Up => new Point(pos.X, pos.Y - 1),
                Direction.Down => new Point(pos.X, pos.Y + 1),
                Direction.Left => new Point(pos.X - 1, pos.Y),
                Direction.Right => new Point(pos.X + 1, pos.Y),
                _ => pos
            };
        }
        
        private Direction GetOpposite(Direction dir)
        {
            return dir switch {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => Direction.None
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_currentState == GameState.Playing)
                {
                    _currentState = GameState.Menu;
                    _gameTimer.Stop();
                }
                InvalidateVisual();
            }

            if (_currentState == GameState.Menu)
            {
                if (e.Key == Key.N)
                {
                    InitGame();
                    _currentState = GameState.Playing;
                    _gameTimer.Start();
                }
                else if (e.Key == Key.Q)
                {
                     if (this.VisualRoot is Window win) win.Close();
                }
            }
            else if (_currentState == GameState.Playing)
            {
                switch (e.Key)
                {
                    case Key.Up: _nextDir = Direction.Up; break;
                    case Key.Down: _nextDir = Direction.Down; break;
                    case Key.Left: _nextDir = Direction.Left; break;
                    case Key.Right: _nextDir = Direction.Right; break;
                }
            }
            else if (_currentState == GameState.GameOver || _currentState == GameState.Win)
            {
                 if (e.Key == Key.Space)
                 {
                    _currentState = GameState.Menu;
                    InvalidateVisual();
                 }
            }

            base.OnKeyDown(e);
            e.Handled = true;
        }

        public override void Render(DrawingContext context)
        {
            context.DrawRectangle(Brushes.Black, null, new Rect(0, 0, Bounds.Width, Bounds.Height));

            if (MapWidth == 0 || MapHeight == 0) return;

            // Calculate Scale
            double scaleX = Bounds.Width / (MapWidth * TileSize);
            double scaleY = Bounds.Height / (MapHeight * TileSize);
            double scale = Math.Min(scaleX, scaleY);
            if (scale <= 0) scale = 1;

            double offsetX = (Bounds.Width - (MapWidth * TileSize * scale)) / 2;
            double offsetY = (Bounds.Height - (MapHeight * TileSize * scale)) / 2;

            var transform = context.PushTransform(Matrix.CreateTranslation(offsetX, offsetY) * Matrix.CreateScale(scale, scale));

            // Draw Map
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    Rect rect = new Rect(x * TileSize, y * TileSize, TileSize, TileSize);
                    if (_map[x, y] == TileType.Wall)
                    {
                        // Nicer walls: Rounded corners rectangle but small
                        // To look like hollow walls, we would need complex logic.
                        // For now, let's just make them simple blue blocks.
                        context.DrawRectangle(_wallBrush, null, rect.Inflate(-1));
                    }
                    else if (_map[x, y] == TileType.Pellet)
                    {
                        context.DrawEllipse(_pelletBrush, null, rect.Center, 2, 2);
                    }
                    else if (_map[x, y] == TileType.PowerPellet)
                    {
                        // Blinking effect?
                        if (DateTime.Now.Millisecond < 500)
                            context.DrawEllipse(_powerPelletBrush, null, rect.Center, 6, 6);
                    }
                }
            }

            // Draw Pacman
            if (_currentState == GameState.Playing || _currentState == GameState.Menu)
            {
                DrawPacman(context, _pacmanPos, _pacmanDir);
            }

            // Draw Ghosts
            if (_ghosts != null)
            {
                foreach (var ghost in _ghosts)
                {
                    DrawGhost(context, ghost);
                }
            }

            transform.Dispose();

            // Draw UI Overlay
            DrawOverlay(context);
        }

        private void DrawPacman(DrawingContext context, Point pos, Direction dir)
        {
            double x = pos.X * TileSize;
            double y = pos.Y * TileSize;
            double centerOffset = TileSize / 2;
            Point center = new Point(x + centerOffset, y + centerOffset);
            double radius = (TileSize / 2) - 2;

            // Rotation based on direction
            double rotation = 0;
            switch (dir)
            {
                case Direction.Up: rotation = -90; break;
                case Direction.Down: rotation = 90; break;
                case Direction.Left: rotation = 180; break;
                case Direction.Right: rotation = 0; break;
            }

            // If mouth is full circle, just draw circle
            if (_mouthAngle <= 1)
            {
                context.DrawEllipse(_pacmanBrush, null, center, radius, radius);
            }
            else
            {
                // Create Pacman Geometry (Pie slice)
                StreamGeometry geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    // Start at mouth top lip
                    double startAngle = _mouthAngle * (Math.PI / 180);
                    // End at mouth bottom lip (going around)
                    double endAngle = (360 - _mouthAngle) * (Math.PI / 180);

                    // We need points on circle.
                    // However, Avalonia ArcTo is simpler.
                    // Start point
                    Point startP = new Point(
                        center.X + radius * Math.Cos(startAngle),
                        center.Y + radius * Math.Sin(startAngle));
                    
                    ctx.BeginFigure(center, true);
                    ctx.LineTo(startP);
                    
                    // Arc around
                    Point endP = new Point(
                        center.X + radius * Math.Cos(endAngle),
                        center.Y + radius * Math.Sin(endAngle));
                    
                    ctx.ArcTo(endP, new Size(radius, radius), 0, true, SweepDirection.Clockwise);
                    ctx.EndFigure(true);
                }

                // Apply rotation
                var angleRad = rotation * Math.PI / 180;
                var matrix = Matrix.CreateTranslation(-center.X, -center.Y) 
                             * Matrix.CreateRotation(angleRad) 
                             * Matrix.CreateTranslation(center.X, center.Y);
                var transform = context.PushTransform(matrix);
                
                context.DrawGeometry(_pacmanBrush, null, geometry);
                transform.Dispose();
            }
        }

        private void DrawGhost(DrawingContext context, Ghost ghost)
        {
            if (ghost.IsDead)
            {
                // Draw eyes only
                DrawGhostEyes(context, ghost);
                return;
            }

            double x = ghost.Pos.X * TileSize;
            double y = ghost.Pos.Y * TileSize;
            
            // Determine Color
            IBrush bodyColor = ghost.NormalColor;
            if (_powerModeTimeLeft > 0)
            {
                // Flash near end
                if (_powerModeTimeLeft < 15 && (_powerModeTimeLeft % 4 < 2))
                    bodyColor = _ghostFrightenedEndingBrush;
                else
                    bodyColor = _ghostFrightenedBrush;
            }

            // Ghost Shape
            StreamGeometry geometry = new StreamGeometry();
            using (var ctx = geometry.Open())
            {
                double w = TileSize - 4; // Width
                double h = TileSize - 4;
                double left = x + 2;
                double top = y + 2;
                double bottom = top + h;

                // Dome head
                ctx.BeginFigure(new Point(left, top + h/2), true);
                ctx.ArcTo(new Point(left + w, top + h/2), new Size(w/2, w/2), 0, false, SweepDirection.Clockwise);
                
                // Right side down
                ctx.LineTo(new Point(left + w, bottom));

                // Wavy bottom (3 waves)
                double waveWidth = w / 3;
                ctx.LineTo(new Point(left + w - waveWidth/2, bottom - 2));
                ctx.LineTo(new Point(left + w - waveWidth, bottom));
                ctx.LineTo(new Point(left + w - 1.5*waveWidth, bottom - 2));
                ctx.LineTo(new Point(left + w - 2*waveWidth, bottom));
                ctx.LineTo(new Point(left + w - 2.5*waveWidth, bottom - 2));
                ctx.LineTo(new Point(left, bottom));

                ctx.EndFigure(true);
            }

            context.DrawGeometry(bodyColor, null, geometry);
            
            // Draw Eyes (if not frightened)
            if (_powerModeTimeLeft <= 0)
            {
                DrawGhostEyes(context, ghost);
            }
            else
            {
                // Draw Frightened Mouth (zig zag line or simple expression)
                // Just small white mouth for now
                 var mouthRect = new Rect(x + 5, y + 12, TileSize - 10, 2);
                 context.DrawRectangle(Brushes.MistyRose, null, mouthRect);
            }
        }

        private void DrawGhostEyes(DrawingContext context, Ghost ghost)
        {
            double x = ghost.Pos.X * TileSize;
            double y = ghost.Pos.Y * TileSize;

            double eyeOffsetX = 0;
            double eyeOffsetY = 0;
            switch(ghost.Dir) {
                case Direction.Up: eyeOffsetY = -2; break;
                case Direction.Down: eyeOffsetY = 2; break;
                case Direction.Left: eyeOffsetX = -2; break;
                case Direction.Right: eyeOffsetX = 2; break;
            }

            // White parts
            context.DrawEllipse(Brushes.White, null, new Point(x + 7, y + 8), 3, 4);
            context.DrawEllipse(Brushes.White, null, new Point(x + 13, y + 8), 3, 4);

            // Pupils (Blue)
            context.DrawEllipse(Brushes.Blue, null, new Point(x + 7 + eyeOffsetX, y + 8 + eyeOffsetY), 1.5, 1.5);
            context.DrawEllipse(Brushes.Blue, null, new Point(x + 13 + eyeOffsetX, y + 8 + eyeOffsetY), 1.5, 1.5);
        }

        private void DrawOverlay(DrawingContext context)
        {
            // Score and Level
            var typeFace = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
            
            var scoreText = new FormattedText($"SCORE: {_score}", 
                System.Globalization.CultureInfo.CurrentCulture, 
                FlowDirection.LeftToRight, typeFace, 16, Brushes.White);
            context.DrawText(scoreText, new Point(10, 5));

            var levelText = new FormattedText($"LEVEL: {_level}", 
                System.Globalization.CultureInfo.CurrentCulture, 
                FlowDirection.LeftToRight, typeFace, 16, Brushes.White);
            context.DrawText(levelText, new Point(10, 25));

            // Lives
            for(int i=0; i<_lives; i++)
            {
                context.DrawEllipse(Brushes.Yellow, null, new Point(Bounds.Width - 20 - (i*25), 20), 8, 8);
            }

            if (_currentState == GameState.Menu)
            {
                // Semi-transparent background
                context.DrawRectangle(new SolidColorBrush(Color.Parse("#AA000000")), null, new Rect(0,0, Bounds.Width, Bounds.Height));
                
                DrawCenteredText(context, "RETRO PACMAN", 40, -60, Brushes.Yellow);
                DrawCenteredText(context, "PRESS 'N' TO START", 20, 0, Brushes.White);
                DrawCenteredText(context, "ARROWS to Move", 14, 40, Brushes.LightGray);
                DrawCenteredText(context, "ESC to Pause/Menu", 14, 60, Brushes.LightGray);
                
                // Draw a preview ghost and pacman
                // Just for flair, maybe not needed strictly
            }
            else if (_currentState == GameState.GameOver)
            {
                context.DrawRectangle(new SolidColorBrush(Color.Parse("#AA000000")), null, new Rect(0,0, Bounds.Width, Bounds.Height));
                DrawCenteredText(context, "GAME OVER", 40, -40, Brushes.Red);
                DrawCenteredText(context, $"SCORE: {_score}", 24, 10, Brushes.White);
                DrawCenteredText(context, "PRESS SPACE", 16, 50, Brushes.White);
            }
        }

        private void DrawCenteredText(DrawingContext context, string text, double size, double yOffset, IBrush? color = null)
        {
             if (color == null) color = Brushes.White;
             var typeFace = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
             var ft = new FormattedText(text, System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, typeFace, size, color);
             
             context.DrawText(ft, new Point((Bounds.Width - ft.Width) / 2, (Bounds.Height / 2) + yOffset));
        }

        private class Ghost
        {
            public Point Pos { get; set; }
            public Direction Dir { get; set; }
            public required IBrush NormalColor { get; set; }
            public bool IsDead { get; set; }
        }
    }
}