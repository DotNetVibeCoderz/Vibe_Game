using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using LiteGame2D.Engine;
using System;
using System.Collections.Generic;

namespace LiteGame2D.Games
{
    public class TetrisGame : IGame
    {
        private const int Rows = 20;
        private const int Cols = 10;
        private const int Size = 25;
        private int[,] _grid; // 0 empty, 1-7 colors
        private Point _currPos;
        private int[,] _currPiece;
        private double _dropTimer;
        private double _dropInterval = 0.5;
        private Random _rnd = new Random();
        private bool _gameOver;
        private bool _keyPressed;

        // Shapes
        private List<int[,]> _shapes = new List<int[,]>
        {
            new int[,] {{1,1,1,1}}, // I
            new int[,] {{1,1},{1,1}}, // O
            new int[,] {{0,1,0},{1,1,1}}, // T
            new int[,] {{1,0,0},{1,1,1}}, // L
            new int[,] {{0,0,1},{1,1,1}}, // J
            new int[,] {{0,1,1},{1,1,0}}, // S
            new int[,] {{1,1,0},{0,1,1}}  // Z
        };
        
        private List<IBrush> _colors = new List<IBrush> {
            Brushes.Transparent, Brushes.Cyan, Brushes.Yellow, Brushes.Purple, Brushes.Orange, Brushes.Blue, Brushes.Green, Brushes.Red
        };

        public void Initialize()
        {
            Reset();
        }

        public void Reset()
        {
            _grid = new int[Rows, Cols];
            SpawnPiece();
            _gameOver = false;
        }

        private void SpawnPiece()
        {
            int idx = _rnd.Next(_shapes.Count);
            _currPiece = (int[,])_shapes[idx].Clone();
            // Color index based on shape index + 1
            for(int i=0; i<_currPiece.GetLength(0); i++)
                for(int j=0; j<_currPiece.GetLength(1); j++)
                    if(_currPiece[i,j] != 0) _currPiece[i,j] = idx + 1;

            _currPos = new Point(Cols / 2 - _currPiece.GetLength(1) / 2, 0);
            
            if (!IsValidPosition(_currPos, _currPiece)) _gameOver = true;
        }

        public void Update(double dt)
        {
            if (_gameOver) { if (Input.IsKeyDown(Key.Space)) Reset(); return; }

            // Input handling (simplified)
            if (Input.IsKeyDown(Key.Left) && !_keyPressed) { Move(-1, 0); _keyPressed = true; }
            else if (Input.IsKeyDown(Key.Right) && !_keyPressed) { Move(1, 0); _keyPressed = true; }
            else if (Input.IsKeyDown(Key.Up) && !_keyPressed) { Rotate(); _keyPressed = true; }
            else if (Input.IsKeyDown(Key.Down)) _dropTimer += dt * 10; // Fast drop
            
            if (!Input.IsKeyDown(Key.Left) && !Input.IsKeyDown(Key.Right) && !Input.IsKeyDown(Key.Up)) 
                _keyPressed = false;

            _dropTimer += dt;
            if (_dropTimer > _dropInterval)
            {
                _dropTimer = 0;
                if (!Move(0, 1))
                {
                    LockPiece();
                    ClearLines();
                    SpawnPiece();
                }
            }
        }

        private bool Move(int dx, int dy)
        {
            var newPos = new Point(_currPos.X + dx, _currPos.Y + dy);
            if (IsValidPosition(newPos, _currPiece))
            {
                _currPos = newPos;
                return true;
            }
            return false;
        }

        private void Rotate()
        {
            int h = _currPiece.GetLength(0);
            int w = _currPiece.GetLength(1);
            int[,] newPiece = new int[w, h];

            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    newPiece[j, h - 1 - i] = _currPiece[i, j];
                }
            }

            if (IsValidPosition(_currPos, newPiece))
                _currPiece = newPiece;
        }

        private bool IsValidPosition(Point pos, int[,] piece)
        {
            for (int i = 0; i < piece.GetLength(0); i++)
            {
                for (int j = 0; j < piece.GetLength(1); j++)
                {
                    if (piece[i, j] != 0)
                    {
                        int x = (int)pos.X + j;
                        int y = (int)pos.Y + i;

                        if (x < 0 || x >= Cols || y >= Rows) return false;
                        if (y >= 0 && _grid[y, x] != 0) return false;
                    }
                }
            }
            return true;
        }

        private void LockPiece()
        {
            for (int i = 0; i < _currPiece.GetLength(0); i++)
            {
                for (int j = 0; j < _currPiece.GetLength(1); j++)
                {
                    if (_currPiece[i, j] != 0)
                    {
                        int y = (int)_currPos.Y + i;
                        int x = (int)_currPos.X + j;
                        if (y >= 0 && y < Rows && x >= 0 && x < Cols)
                            _grid[y, x] = _currPiece[i, j];
                    }
                }
            }
        }

        private void ClearLines()
        {
            for (int i = Rows - 1; i >= 0; i--)
            {
                bool full = true;
                for (int j = 0; j < Cols; j++)
                    if (_grid[i, j] == 0) full = false;

                if (full)
                {
                    for (int k = i; k > 0; k--)
                        for (int j = 0; j < Cols; j++)
                            _grid[k, j] = _grid[k - 1, j];
                    
                    for (int j = 0; j < Cols; j++) _grid[0, j] = 0;
                    i++; // Check same line again
                }
            }
        }

        public void Draw(DrawingContext context, Size screenSize)
        {
            double offsetX = (screenSize.Width - Cols * Size) / 2;
            double offsetY = 20;

            // Draw Board
            context.DrawRectangle(null, new Pen(Brushes.White, 2), new Rect(offsetX, offsetY, Cols * Size, Rows * Size));

            // Draw Grid
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    if (_grid[i, j] != 0)
                        context.FillRectangle(_colors[_grid[i, j]], new Rect(offsetX + j * Size, offsetY + i * Size, Size - 1, Size - 1));

            // Draw Current Piece
            for (int i = 0; i < _currPiece.GetLength(0); i++)
                for (int j = 0; j < _currPiece.GetLength(1); j++)
                    if (_currPiece[i, j] != 0)
                        context.FillRectangle(_colors[_currPiece[i, j]], new Rect(offsetX + (_currPos.X + j) * Size, offsetY + (_currPos.Y + i) * Size, Size - 1, Size - 1));

             if (_gameOver)
                 context.DrawText(new FormattedText("GAME OVER", System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 40, Brushes.White), new Point(offsetX, 200));
        }
    }
}