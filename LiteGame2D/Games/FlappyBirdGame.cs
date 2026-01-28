using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using LiteGame2D.Engine;
using System;
using System.Collections.Generic;

namespace LiteGame2D.Games
{
    public class FlappyBirdGame : IGame
    {
        private double _birdY;
        private double _velocity;
        private const double Gravity = 1000;
        private const double JumpStrength = -350;
        private List<Rect> _pipes;
        private double _pipeSpawnTimer;
        private bool _gameOver;
        private Random _rnd = new Random();
        
        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            _birdY = 300;
            _velocity = 0;
            _pipes = new List<Rect>();
            _pipeSpawnTimer = 0;
            _gameOver = false;
        }

        public void Update(double dt)
        {
            if (_gameOver)
            {
                if (Input.IsKeyDown(Key.Space)) Reset();
                return;
            }

            if (Input.IsKeyDown(Key.Space))
            {
                _velocity = JumpStrength;
            }

            _velocity += Gravity * dt;
            _birdY += _velocity * dt;

            // Spawn Pipes
            _pipeSpawnTimer += dt;
            if (_pipeSpawnTimer > 2.0)
            {
                SpawnPipe();
                _pipeSpawnTimer = 0;
            }

            // Move Pipes
            for (int i = 0; i < _pipes.Count; i++)
            {
                var r = _pipes[i];
                _pipes[i] = new Rect(r.X - 150 * dt, r.Y, r.Width, r.Height);
            }

            // Cleanup Pipes
            _pipes.RemoveAll(p => p.Right < 0);

            // Collision
            var birdRect = new Rect(100, _birdY, 30, 30);
            if (_birdY > 600 || _birdY < 0) _gameOver = true;

            foreach (var p in _pipes)
            {
                if (birdRect.Intersects(p)) _gameOver = true;
            }
        }

        private void SpawnPipe()
        {
            double gap = 150;
            double height = _rnd.Next(50, 400);
            _pipes.Add(new Rect(800, 0, 50, height)); // Top pipe
            _pipes.Add(new Rect(800, height + gap, 50, 600 - (height + gap))); // Bottom pipe
        }

        public void Draw(DrawingContext context, Size screenSize)
        {
            // Draw Bird (Sprite-like)
            var birdRect = new Rect(100, _birdY, 34, 24);
            
            // Body (Yellow Oval)
            context.DrawEllipse(Brushes.Yellow, null, birdRect.Center, birdRect.Width/2, birdRect.Height/2);
            context.DrawEllipse(null, new Pen(Brushes.Black, 2), birdRect.Center, birdRect.Width/2, birdRect.Height/2);

            // Eye (White with Black Pupil)
            var eyeRect = new Rect(birdRect.Right - 12, birdRect.Top + 4, 10, 10);
            context.DrawEllipse(Brushes.White, null, eyeRect.Center, eyeRect.Width/2, eyeRect.Height/2);
            context.DrawEllipse(null, new Pen(Brushes.Black, 1), eyeRect.Center, eyeRect.Width/2, eyeRect.Height/2);
            var pupilRect = new Rect(eyeRect.Center.X + 2, eyeRect.Center.Y - 1, 3, 3);
            context.DrawEllipse(Brushes.Black, null, pupilRect.Center, pupilRect.Width/2, pupilRect.Height/2);

            // Beak (Orange Triangle)
            var beakGeo = new StreamGeometry();
            using(var ctx = beakGeo.Open())
            {
                ctx.BeginFigure(new Point(birdRect.Right - 2, birdRect.Center.Y + 2), true);
                ctx.LineTo(new Point(birdRect.Right + 8, birdRect.Center.Y + 6));
                ctx.LineTo(new Point(birdRect.Right - 2, birdRect.Center.Y + 10));
                ctx.EndFigure(true);
            }
            context.DrawGeometry(Brushes.Orange, new Pen(Brushes.Black, 1), beakGeo);

            // Wing (Small white oval)
            var wingRect = new Rect(birdRect.Center.X - 8, birdRect.Center.Y - 2, 12, 8);
            context.DrawEllipse(Brushes.White, null, wingRect.Center, wingRect.Width/2, wingRect.Height/2);
            context.DrawEllipse(null, new Pen(Brushes.Black, 1), wingRect.Center, wingRect.Width/2, wingRect.Height/2);

            // Draw Pipes
            foreach (var p in _pipes)
            {
                // Pipe Body
                context.FillRectangle(Brushes.Green, p);
                context.DrawRectangle(new Pen(Brushes.DarkGreen, 3), p);
                
                // Pipe Cap details (Top/Bottom visual)
                if (p.Y == 0) // Top Pipe
                    context.FillRectangle(Brushes.LimeGreen, new Rect(p.X - 2, p.Bottom - 20, p.Width + 4, 20));
                else // Bottom Pipe
                    context.FillRectangle(Brushes.LimeGreen, new Rect(p.X - 2, p.Y, p.Width + 4, 20));
            }

            if (_gameOver)
                 context.DrawText(new FormattedText("GAME OVER", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 40, Brushes.White), new Point(300, 300));
        }
    }
}