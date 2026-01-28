using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using LiteGame2D.Engine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteGame2D.Games
{
    public class SnakeGame : IGame
    {
        private const int CellSize = 20;
        private List<Point> _snake;
        private Point _food;
        private Point _direction;
        private double _moveTimer;
        private double _moveInterval = 0.1;
        private Random _rnd = new Random();
        private bool _gameOver;
        private IBrush _snakeBrush = Brushes.Green;
        private IBrush _foodBrush = Brushes.Red;

        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            _snake = new List<Point> { new Point(10, 10), new Point(9, 10), new Point(8, 10) };
            _direction = new Point(1, 0);
            _moveTimer = 0;
            _gameOver = false;
            SpawnFood();
        }

        private void SpawnFood()
        {
            _food = new Point(_rnd.Next(0, 30), _rnd.Next(0, 20));
        }

        public void Update(double deltaTime)
        {
            if (_gameOver)
            {
                if (Input.IsKeyDown(Key.Space)) Reset();
                return;
            }

            if (Input.IsKeyDown(Key.Up) && _direction.Y == 0) _direction = new Point(0, -1);
            else if (Input.IsKeyDown(Key.Down) && _direction.Y == 0) _direction = new Point(0, 1);
            else if (Input.IsKeyDown(Key.Left) && _direction.X == 0) _direction = new Point(-1, 0);
            else if (Input.IsKeyDown(Key.Right) && _direction.X == 0) _direction = new Point(1, 0);

            _moveTimer += deltaTime;
            if (_moveTimer >= _moveInterval)
            {
                _moveTimer = 0;
                Move();
            }
        }

        private void Move()
        {
            var head = _snake[0];
            var newHead = new Point(head.X + _direction.X, head.Y + _direction.Y);

            // Collision with walls
            if (newHead.X < 0 || newHead.X >= 40 || newHead.Y < 0 || newHead.Y >= 30)
            {
                _gameOver = true;
                return;
            }

            // Collision with self
            if (_snake.Any(s => s.X == newHead.X && s.Y == newHead.Y))
            {
                _gameOver = true;
                return;
            }

            _snake.Insert(0, newHead);

            if (newHead.X == _food.X && newHead.Y == _food.Y)
            {
                SpawnFood();
                // Speed up slightly
                if(_moveInterval > 0.05) _moveInterval -= 0.001;
            }
            else
            {
                _snake.RemoveAt(_snake.Count - 1);
            }
        }

        public void Draw(DrawingContext context, Size screenSize)
        {
            // Draw Grid (optional, skipping for performance)
            
            // Draw Snake
            foreach (var part in _snake)
            {
                context.FillRectangle(_snakeBrush, new Rect(part.X * CellSize, part.Y * CellSize, CellSize - 1, CellSize - 1));
            }

            // Draw Food
            context.FillRectangle(_foodBrush, new Rect(_food.X * CellSize, _food.Y * CellSize, CellSize - 1, CellSize - 1));

            if (_gameOver)
            {
                // Simple text rendering placeholder
                context.DrawText(new FormattedText("GAME OVER\nPress Space", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 40, Brushes.White), new Point(screenSize.Width / 2 - 100, screenSize.Height / 2));
            }
        }
    }
}