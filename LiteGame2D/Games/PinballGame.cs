using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using LiteGame2D.Engine;
using System;
using System.Collections.Generic;

namespace LiteGame2D.Games
{
    public class PinballGame : IGame
    {
        private Point _ballPos;
        private Point _ballVel;
        private Rect _paddle;
        private List<Rect> _bumpers;
        private List<Point> _coins;
        private double _gravity = 500;
        private bool _gameOver;
        private int _score;
        private Random _rnd = new Random();

        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            _ballPos = new Point(400, 100);
            _ballVel = new Point(200, 0); // Start with some push
            _paddle = new Rect(350, 550, 100, 20);
            _bumpers = new List<Rect>
            {
                new Rect(200, 200, 50, 50),
                new Rect(550, 200, 50, 50),
                new Rect(375, 100, 50, 50)
            };
            
            _coins = new List<Point>();
            for(int i=0; i<10; i++)
            {
                _coins.Add(new Point(_rnd.Next(50, 750), _rnd.Next(50, 400)));
            }

            _score = 0;
            _gameOver = false;
        }

        public void Update(double dt)
        {
            if (_gameOver)
            {
                if (Input.IsKeyDown(Key.Space)) Reset();
                return;
            }

            // Paddle Control
            if (Input.IsKeyDown(Key.Left)) _paddle = new Rect(_paddle.X - 500 * dt, _paddle.Y, _paddle.Width, _paddle.Height);
            if (Input.IsKeyDown(Key.Right)) _paddle = new Rect(_paddle.X + 500 * dt, _paddle.Y, _paddle.Width, _paddle.Height);

            // Clamp Paddle
            if (_paddle.X < 0) _paddle = new Rect(0, _paddle.Y, _paddle.Width, _paddle.Height);
            if (_paddle.Right > 800) _paddle = new Rect(800 - _paddle.Width, _paddle.Y, _paddle.Width, _paddle.Height);

            // Physics
            _ballVel = new Point(_ballVel.X, _ballVel.Y + _gravity * dt);
            _ballPos = new Point(_ballPos.X + _ballVel.X * dt, _ballPos.Y + _ballVel.Y * dt);

            // Walls
            if (_ballPos.X < 10) { _ballPos = new Point(10, _ballPos.Y); _ballVel = new Point(-_ballVel.X * 0.8, _ballVel.Y); }
            if (_ballPos.X > 790) { _ballPos = new Point(790, _ballPos.Y); _ballVel = new Point(-_ballVel.X * 0.8, _ballVel.Y); }
            if (_ballPos.Y < 10) { _ballPos = new Point(_ballPos.X, 10); _ballVel = new Point(_ballVel.X, -_ballVel.Y * 0.8); }
            if (_ballPos.Y > 600) _gameOver = true;

            // Paddle Collision
            var ballRect = new Rect(_ballPos.X - 10, _ballPos.Y - 10, 20, 20);
            if (ballRect.Intersects(_paddle) && _ballVel.Y > 0)
            {
                _ballVel = new Point(_ballVel.X + (_ballPos.X - _paddle.Center.X) * 5, -Math.Abs(_ballVel.Y) * 1.05); // Add spin and bounce
            }

            // Bumper Collision
            foreach (var b in _bumpers)
            {
                if (ballRect.Intersects(b))
                {
                    // Simple AABB Bounce
                    var intersect = ballRect.Intersect(b);
                    if (intersect.Width < intersect.Height) _ballVel = new Point(-_ballVel.X, _ballVel.Y);
                    else _ballVel = new Point(_ballVel.X, -_ballVel.Y);
                    _score += 10;
                }
            }

            // Coin Collection
            for (int i = _coins.Count - 1; i >= 0; i--)
            {
                var c = _coins[i];
                // Distance check (radius of ball 10 + radius of coin 8 = 18)
                double dist = Math.Sqrt(Math.Pow(_ballPos.X - c.X, 2) + Math.Pow(_ballPos.Y - c.Y, 2));
                if (dist < 20)
                {
                    _coins.RemoveAt(i);
                    _score += 50;
                }
            }
            
            // Respawn coins if all collected
            if (_coins.Count == 0)
            {
                 for(int i=0; i<5; i++) _coins.Add(new Point(_rnd.Next(50, 750), _rnd.Next(50, 400)));
            }
        }

        public void Draw(DrawingContext context, Size screenSize)
        {
            // Paddle
            context.FillRectangle(Brushes.Blue, _paddle);
            context.DrawRectangle(new Pen(Brushes.LightBlue, 2), _paddle);

            // Ball
            var ballR = new Rect(_ballPos.X - 10, _ballPos.Y - 10, 20, 20);
            context.DrawEllipse(Brushes.Silver, null, ballR.Center, ballR.Width/2, ballR.Height/2);
            
            var shineR = new Rect(_ballPos.X - 4, _ballPos.Y - 6, 6, 6);
            context.DrawEllipse(Brushes.White, null, shineR.Center, shineR.Width/2, shineR.Height/2);

            // Bumpers
            foreach (var b in _bumpers) 
            {
                context.FillRectangle(Brushes.Red, b);
                context.DrawRectangle(new Pen(Brushes.DarkRed, 3), b);
                var innerB = b.Inflate(-10);
                context.DrawEllipse(Brushes.Orange, null, innerB.Center, innerB.Width/2, innerB.Height/2);
            }

            // Coins
            foreach (var c in _coins)
            {
                var r = new Rect(c.X - 8, c.Y - 8, 16, 16);
                context.DrawEllipse(Brushes.Gold, null, r.Center, r.Width/2, r.Height/2);
                context.DrawEllipse(null, new Pen(Brushes.Goldenrod, 2), r.Center, r.Width/2, r.Height/2);
                context.DrawText(new FormattedText("$", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 10, Brushes.Black), new Point(c.X - 3, c.Y - 6));
            }

            // UI
            context.DrawText(new FormattedText($"Score: {_score}", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 20, Brushes.White), new Point(10, 10));

            if (_gameOver)
                context.DrawText(new FormattedText("GAME OVER", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 40, Brushes.White), new Point(300, 300));
        }
    }
}