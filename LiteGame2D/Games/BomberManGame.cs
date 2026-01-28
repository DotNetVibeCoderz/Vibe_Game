using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using LiteGame2D.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteGame2D.Games
{
    public class BomberManGame : IGame
    {
        private const int Size = 40;
        private Point _playerPos;
        private List<Point> _walls;
        private List<Bomb> _bombs;
        private List<Explosion> _explosions;
        private List<Enemy> _enemies;
        private bool _gameOver;
        private bool _spacePressed;
        private Random _rnd = new Random();
        
        private class Bomb { public Point Pos; public double Timer; }
        private class Explosion { public Point Pos; public double Timer; }
        private class Enemy 
        { 
            public Point Pos; 
            public double MoveTimer; 
            // 0: Up, 1: Down, 2: Left, 3: Right
            public int Direction; 
        }

        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            _playerPos = new Point(1, 1);
            _walls = new List<Point>();
            _bombs = new List<Bomb>();
            _explosions = new List<Explosion>();
            _enemies = new List<Enemy>();
            _gameOver = false;
            
            // Generate Walls
            for(int i=0; i<20; i++)
                for(int j=0; j<15; j++)
                {
                    if (i == 0 || i == 19 || j == 0 || j == 14) _walls.Add(new Point(i, j)); // Borders
                    else if (i % 2 == 0 && j % 2 == 0) _walls.Add(new Point(i, j)); // Pillars
                }

            // Generate Enemies
            for(int k=0; k<5; k++)
            {
                while(true)
                {
                    int ex = _rnd.Next(1, 19);
                    int ey = _rnd.Next(1, 14);
                    var p = new Point(ex, ey);
                    // Don't spawn on walls, borders, or too close to player
                    if (!_walls.Contains(p) && (Math.Abs(ex - 1) > 2 || Math.Abs(ey - 1) > 2))
                    {
                        _enemies.Add(new Enemy { Pos = p, MoveTimer = 0, Direction = _rnd.Next(4) });
                        break;
                    }
                }
            }
        }

        public void Update(double dt)
        {
            if (_gameOver) { if (Input.IsKeyDown(Key.Space) && !_spacePressed) Reset(); _spacePressed = Input.IsKeyDown(Key.Space); return; }

            if (Input.IsKeyDown(Key.Space) && !_spacePressed)
            {
                if (!_bombs.Any(b => b.Pos == _playerPos))
                    _bombs.Add(new Bomb { Pos = _playerPos, Timer = 2.0 });
            }
            _spacePressed = Input.IsKeyDown(Key.Space);

            if (Input.IsKeyDown(Key.Left)) TryMovePlayer(-1, 0, dt);
            else if (Input.IsKeyDown(Key.Right)) TryMovePlayer(1, 0, dt);
            else if (Input.IsKeyDown(Key.Up)) TryMovePlayer(0, -1, dt);
            else if (Input.IsKeyDown(Key.Down)) TryMovePlayer(0, 1, dt);

            // Update Enemies
            UpdateEnemies(dt);

            // Update Bombs
            for (int i = _bombs.Count - 1; i >= 0; i--)
            {
                _bombs[i].Timer -= dt;
                if (_bombs[i].Timer <= 0)
                {
                    Explode(_bombs[i].Pos);
                    _bombs.RemoveAt(i);
                }
            }

            // Update Explosions
            for (int i = _explosions.Count - 1; i >= 0; i--)
            {
                _explosions[i].Timer -= dt;
                if (_explosions[i].Timer <= 0) _explosions.RemoveAt(i);
                else
                {
                    // Check kill player
                    if (IsColliding(_playerPos, _explosions[i].Pos))
                        _gameOver = true;

                    // Check kill enemies
                    for (int j = _enemies.Count - 1; j >= 0; j--)
                    {
                        if (IsColliding(_enemies[j].Pos, _explosions[i].Pos))
                            _enemies.RemoveAt(j);
                    }
                }
            }

            // Check Player vs Enemy
            foreach(var e in _enemies)
            {
                if (IsColliding(_playerPos, e.Pos)) _gameOver = true;
            }
        }

        private bool IsColliding(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) < 0.1 && Math.Abs(p1.Y - p2.Y) < 0.1;
        }

        private void UpdateEnemies(double dt)
        {
            foreach(var e in _enemies)
            {
                e.MoveTimer -= dt;
                if (e.MoveTimer <= 0)
                {
                    e.MoveTimer = 0.5; // Move every 0.5s
                    
                    int dx = 0, dy = 0;
                    switch(e.Direction)
                    {
                        case 0: dy = -1; break; // Up
                        case 1: dy = 1; break;  // Down
                        case 2: dx = -1; break; // Left
                        case 3: dx = 1; break;  // Right
                    }

                    var newPos = new Point(e.Pos.X + dx, e.Pos.Y + dy);
                    
                    // Check collision
                    bool hitWall = _walls.Contains(newPos) || _bombs.Any(b => b.Pos == newPos) || newPos.X < 0 || newPos.X >= 20 || newPos.Y < 0 || newPos.Y >= 15;
                    
                    if (!hitWall)
                    {
                        e.Pos = newPos;
                    }
                    else
                    {
                        // Change direction randomly
                        e.Direction = _rnd.Next(4);
                    }
                }
            }
        }

        private double _moveTimer;
        private void TryMovePlayer(int dx, int dy, double dt)
        {
            _moveTimer += dt;
            if (_moveTimer < 0.15) return;
            _moveTimer = 0;

            var newPos = new Point(Math.Round(_playerPos.X + dx), Math.Round(_playerPos.Y + dy));
            if (!_walls.Contains(newPos) && !_bombs.Any(b => b.Pos == newPos))
            {
                _playerPos = newPos;
            }
        }

        private void Explode(Point center)
        {
            // Center
            _explosions.Add(new Explosion { Pos = center, Timer = 0.5 });
            
            // 4 Directions
            int range = 2;
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            for(int d=0; d<4; d++)
            {
                for(int r=1; r<=range; r++)
                {
                    var p = new Point(center.X + dx[d] * r, center.Y + dy[d] * r);
                    if (_walls.Contains(p)) break; // Stop at walls
                    _explosions.Add(new Explosion { Pos = p, Timer = 0.5 });
                }
            }
        }

        public void Draw(DrawingContext context, Size screenSize)
        {
            // Walls
            foreach (var w in _walls) 
            {
                var r = new Rect(w.X * Size, w.Y * Size, Size, Size);
                context.FillRectangle(Brushes.Gray, r);
                context.DrawRectangle(new Pen(Brushes.DarkGray, 2), r); 
                context.DrawLine(new Pen(Brushes.DarkGray, 2), r.TopLeft + new Point(0, Size/2), r.TopRight + new Point(0, Size/2));
            }
            
            // Bombs
            foreach (var b in _bombs) 
            {
                var center = new Point(b.Pos.X * Size + Size/2, b.Pos.Y * Size + Size/2);
                var r = new Rect(b.Pos.X * Size + 5, b.Pos.Y * Size + 5, Size - 10, Size - 10);
                context.DrawEllipse(Brushes.Black, null, r.Center, r.Width/2, r.Height/2);
                
                // Fuse
                context.DrawLine(new Pen(Brushes.SaddleBrown, 2), center + new Point(0, -10), center + new Point(5, -15));

                // Red blinking
                if((int)(b.Timer * 10) % 2 == 0)
                {
                    var rRed = new Rect(center.X - 3, center.Y - 3, 6, 6);
                    context.DrawEllipse(Brushes.Red, null, rRed.Center, rRed.Width/2, rRed.Height/2);
                }
            }

            // Explosions
            foreach (var e in _explosions) 
            {
                var r = new Rect(e.Pos.X * Size, e.Pos.Y * Size, Size, Size);
                context.FillRectangle(Brushes.OrangeRed, r);
                var rIn = r.Inflate(-5);
                context.DrawEllipse(Brushes.Yellow, null, rIn.Center, rIn.Width/2, rIn.Height/2);
            }

            // Enemies (Ghost/Monster Sprite)
            foreach (var e in _enemies)
            {
                var r = new Rect(e.Pos.X * Size, e.Pos.Y * Size, Size, Size);
                var body = r.Inflate(-5);
                
                // Body (Red Blob)
                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(new Point(body.Left, body.Bottom), true);
                    ctx.LineTo(new Point(body.Left, body.Top + 10));
                    ctx.ArcTo(new Point(body.Right, body.Top + 10), new Size(body.Width/2, body.Height/2), 0, false, SweepDirection.Clockwise);
                    ctx.LineTo(new Point(body.Right, body.Bottom));
                    // Jagged bottom
                    ctx.LineTo(new Point(body.Right - 5, body.Bottom - 5));
                    ctx.LineTo(new Point(body.Center.X, body.Bottom));
                    ctx.LineTo(new Point(body.Left + 5, body.Bottom - 5));
                    ctx.EndFigure(true);
                }
                context.DrawGeometry(Brushes.Red, null, geometry);

                // Eyes
                var eyeLeft = new Rect(body.Left + 5, body.Top + 10, 8, 8);
                var eyeRight = new Rect(body.Right - 13, body.Top + 10, 8, 8);
                var pupilLeft = new Rect(body.Left + 7, body.Top + 12, 3, 3);
                var pupilRight = new Rect(body.Right - 11, body.Top + 12, 3, 3);
                
                context.DrawEllipse(Brushes.White, null, eyeLeft.Center, eyeLeft.Width/2, eyeLeft.Height/2);
                context.DrawEllipse(Brushes.White, null, eyeRight.Center, eyeRight.Width/2, eyeRight.Height/2);
                context.DrawEllipse(Brushes.Black, null, pupilLeft.Center, pupilLeft.Width/2, pupilLeft.Height/2);
                context.DrawEllipse(Brushes.Black, null, pupilRight.Center, pupilRight.Width/2, pupilRight.Height/2);
            }

            // Player (Bomberman style)
            var pr = new Rect(_playerPos.X * Size, _playerPos.Y * Size, Size, Size);
            var pBody = pr.Inflate(-5);
            
            // Body
            var rBody = new Rect(pBody.Center.X - 8, pBody.Bottom - 15, 16, 12);
            context.DrawEllipse(Brushes.Blue, null, rBody.Center, rBody.Width/2, rBody.Height/2);
            
            // Head
            var rHead = new Rect(pBody.Center.X - 10, pBody.Top + 2, 20, 20);
            context.DrawEllipse(Brushes.White, null, rHead.Center, rHead.Width/2, rHead.Height/2);
            
            // Face
            context.FillRectangle(Brushes.Fuchsia, new Rect(pBody.Center.X - 6, pBody.Top + 8, 12, 8));
            
            // Hat bobble
            var rBobble = new Rect(pBody.Center.X - 3, pBody.Top - 2, 6, 6);
            context.DrawEllipse(Brushes.Pink, null, rBobble.Center, rBobble.Width/2, rBobble.Height/2);

            if (_gameOver)
                context.DrawText(new FormattedText("GAME OVER", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 40, Brushes.White), new Point(200, 200));
        }
    }
}