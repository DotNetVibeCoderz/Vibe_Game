using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using LiteGame2D.Engine;
using System;
using System.Collections.Generic;

namespace LiteGame2D.Games
{
    public class TRexGame : IGame
    {
        private double _dinoY;
        private double _velocity;
        private const double GroundY = 400;
        private const double Gravity = 1200;
        private const double JumpForce = -600;
        private List<Rect> _obstacles;
        private double _spawnTimer;
        private bool _isGrounded;
        private bool _gameOver;
        private bool _spacePressed;
        private int _score;
        private Random _rnd = new Random();
        
        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            _dinoY = GroundY;
            _velocity = 0;
            _obstacles = new List<Rect>();
            _gameOver = false;
            _score = 0;
        }

        public void Update(double dt)
        {
             if (_gameOver)
            {
                if (Input.IsKeyDown(Key.Space) && !_spacePressed) Reset();
                _spacePressed = Input.IsKeyDown(Key.Space);
                return;
            }

            _score++;

            bool spaceDown = Input.IsKeyDown(Key.Space);

            if (spaceDown && !_spacePressed && _isGrounded)
            {
                _velocity = JumpForce;
                _isGrounded = false;
            }
            _spacePressed = spaceDown;

            if (!_isGrounded)
            {
                _velocity += Gravity * dt;
                _dinoY += _velocity * dt;

                if (_dinoY >= GroundY)
                {
                    _dinoY = GroundY;
                    _velocity = 0;
                    _isGrounded = true;
                }
            }

            _spawnTimer += dt;
            if (_spawnTimer > 1.5 + _rnd.NextDouble()) 
            {
                _obstacles.Add(new Rect(800, GroundY + 10, 20, 40)); // Cactus base size
                _spawnTimer = 0;
            }

            for(int i=0; i<_obstacles.Count; i++)
            {
                var r = _obstacles[i];
                double speed = 300 + (_score / 100);
                _obstacles[i] = new Rect(r.X - speed * dt, r.Y, r.Width, r.Height);
            }
            _obstacles.RemoveAll(x => x.Right < 0);

            var dinoRect = new Rect(100, _dinoY, 40, 50);
            foreach(var obs in _obstacles)
            {
                if (dinoRect.Intersects(obs)) _gameOver = true;
            }
        }

        public void Draw(DrawingContext context, Size screenSize)
        {
            // Ground
            context.DrawLine(new Pen(Brushes.White, 2), new Point(0, GroundY + 50), new Point(800, GroundY + 50));
            // Some random dots on ground
            for(int i=0; i<800; i+=100) context.FillRectangle(Brushes.Gray, new Rect(i + (_score % 100), GroundY + 55, 4, 2));

            // Dino (Draw Geometry)
            var dinoRect = new Rect(100, _dinoY, 40, 50);
            var dinoColor = Brushes.Gray;
            if (_gameOver) dinoColor = Brushes.Red;

            // Simple blocky dino shape
            // Head
            context.FillRectangle(dinoColor, new Rect(dinoRect.X + 20, dinoRect.Y, 20, 15));
            // Eye
            context.FillRectangle(Brushes.White, new Rect(dinoRect.X + 28, dinoRect.Y + 2, 4, 4));
            // Snout
            context.FillRectangle(dinoColor, new Rect(dinoRect.X + 20, dinoRect.Y + 15, 10, 5));
            // Body
            context.FillRectangle(dinoColor, new Rect(dinoRect.X + 10, dinoRect.Y + 15, 15, 25));
            // Tail
            context.FillRectangle(dinoColor, new Rect(dinoRect.X, dinoRect.Y + 20, 10, 10));
            // Arm
            context.FillRectangle(dinoColor, new Rect(dinoRect.X + 25, dinoRect.Y + 20, 5, 5));
            // Legs
            if ((_score / 10) % 2 == 0 || !_isGrounded) // Run animation
            {
                context.FillRectangle(dinoColor, new Rect(dinoRect.X + 12, dinoRect.Y + 40, 5, 10));
                context.FillRectangle(dinoColor, new Rect(dinoRect.X + 22, dinoRect.Y + 40, 5, 5));
            }
            else
            {
                context.FillRectangle(dinoColor, new Rect(dinoRect.X + 12, dinoRect.Y + 40, 5, 5));
                context.FillRectangle(dinoColor, new Rect(dinoRect.X + 22, dinoRect.Y + 40, 5, 10));
            }


            // Obstacles (Cactus)
            foreach(var obs in _obstacles)
            {
                // Main stem
                context.FillRectangle(Brushes.Green, obs);
                // Branches
                context.FillRectangle(Brushes.Green, new Rect(obs.X - 5, obs.Y + 10, 5, 10));
                context.FillRectangle(Brushes.Green, new Rect(obs.X - 5, obs.Y + 5, 10, 5)); // Left arm connection
                
                context.FillRectangle(Brushes.Green, new Rect(obs.Right, obs.Y + 5, 5, 10));
                context.FillRectangle(Brushes.Green, new Rect(obs.Right - 5, obs.Y + 15, 10, 5)); // Right arm connection
            }

            // Score
            context.DrawText(new FormattedText($"HI {(_score/10):D5}", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 20, Brushes.Gray), new Point(650, 20));

            if (_gameOver)
                 context.DrawText(new FormattedText("GAME OVER", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 40, Brushes.White), new Point(300, 200));
        }
    }
}