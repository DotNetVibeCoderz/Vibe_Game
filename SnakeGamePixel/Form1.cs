using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GameTimer = System.Windows.Forms.Timer; // Fix ambiguity

namespace SnakeGamePixel
{
    // Enum untuk arah pergerakan ular
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public partial class Form1 : Form
    {
        // --- Game Configuration & State ---
        private List<Point> snake;      // Badan ular
        private Point food;             // Makanan
        private List<Point> obstacles;  // Rintangan (Level mechanic)
        private Direction currentDirection;
        private Direction nextDirection; // Buffer untuk input keyboard
        
        private GameTimer gameTimer;
        private Random rand;
        
        private int score;
        private int level;
        private bool isGameOver;

        // --- Visual Settings ---
        private const int TileSize = 25; // Ukuran 1 "Pixel"
        private const int BoardWidth = 25; // Jumlah tile ke samping
        private const int BoardHeight = 20; // Jumlah tile ke bawah

        public Form1()
        {
            InitializeComponent();
            InitializeGameSettings();
        }

        private void InitializeGameSettings()
        {
            // Setup Window
            this.Text = "Snake Game - Retro Pixel by Jacky";
            this.ClientSize = new Size(BoardWidth * TileSize, BoardHeight * TileSize + 40); // +40 untuk status bar logic
            this.DoubleBuffered = true; // Mencegah kedip-kedip (flickering)
            this.BackColor = Color.Black;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Setup Timer
            gameTimer = new GameTimer();
            gameTimer.Interval = 200; // Speed awal (ms)
            gameTimer.Tick += GameLoop;

            rand = new Random();
            
            // Start Game Pertama kali
            StartGame();
        }

        private void StartGame()
        {
            // Reset Variable
            isGameOver = false;
            score = 0;
            level = 1;
            gameTimer.Interval = 200;
            
            // Posisi awal ular di tengah
            snake = new List<Point>();
            snake.Add(new Point(BoardWidth / 2, BoardHeight / 2));
            snake.Add(new Point(BoardWidth / 2, (BoardHeight / 2) + 1)); // Ekor

            currentDirection = Direction.Up;
            nextDirection = Direction.Up;

            obstacles = new List<Point>();
            
            GenerateFood();
            gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (isGameOver) return;

            currentDirection = nextDirection;
            Point head = snake[0];
            Point newHead = head;

            // Tentukan posisi kepala baru
            switch (currentDirection)
            {
                case Direction.Up: newHead.Y--; break;
                case Direction.Down: newHead.Y++; break;
                case Direction.Left: newHead.X--; break;
                case Direction.Right: newHead.X++; break;
            }

            // 1. Cek Tabrakan dengan Dinding
            if (newHead.X < 0 || newHead.Y < 0 || newHead.X >= BoardWidth || newHead.Y >= BoardHeight)
            {
                GameOver();
                return;
            }

            // 2. Cek Tabrakan dengan Badan Sendiri
            if (snake.Contains(newHead))
            {
                GameOver();
                return;
            }

            // 3. Cek Tabrakan dengan Obstacle (Level challenge)
            if (obstacles.Contains(newHead))
            {
                GameOver();
                return;
            }

            // Tambahkan kepala baru
            snake.Insert(0, newHead);

            // 4. Cek Makanan
            if (newHead == food)
            {
                score += 10;
                CheckLevelUp(); // Cek apakah naik level
                GenerateFood();
                // Kalau makan, ekor tidak dibuang (ular memanjang)
            }
            else
            {
                // Kalau tidak makan, buang ekor lama (gerak standard)
                snake.RemoveAt(snake.Count - 1);
            }

            this.Invalidate(); // Refresh / Gambar ulang layar
        }

        private void CheckLevelUp()
        {
            // Naik level setiap 50 poin
            int newLevel = (score / 50) + 1;
            
            if (newLevel > level)
            {
                level = newLevel;
                // Challenge: Tambah kecepatan
                if (gameTimer.Interval > 50) 
                    gameTimer.Interval -= 20;

                // Challenge: Tambah Obstacle
                GenerateObstacle();
            }
        }

        private void GenerateObstacle()
        {
            // Buat obstacle acak yang tidak menimpa ular atau makanan
            Point obstaclePos;
            do
            {
                obstaclePos = new Point(rand.Next(0, BoardWidth), rand.Next(0, BoardHeight));
            } while (snake.Contains(obstaclePos) || obstaclePos == food || obstacles.Contains(obstaclePos) || GameDistance(snake[0], obstaclePos) < 5);
            
            obstacles.Add(obstaclePos);
        }

        private double GameDistance(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private void GenerateFood()
        {
            Point foodPos;
            do
            {
                foodPos = new Point(rand.Next(0, BoardWidth), rand.Next(0, BoardHeight));
            } while (snake.Contains(foodPos) || obstacles.Contains(foodPos));
            
            food = foodPos;
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer.Stop();
            MessageBox.Show($"Game Over! Score: {score}, Level: {level}\nTekan Enter untuk Main Lagi.", "Jacky's Snake Game");
        }

        // --- Input Handling ---
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (isGameOver)
            {
                if (e.KeyCode == Keys.Enter) StartGame();
                return;
            }

            // Cegah ular balik arah 180 derajat langsung
            switch (e.KeyCode)
            {
                case Keys.Up: 
                    if (currentDirection != Direction.Down) nextDirection = Direction.Up; 
                    break;
                case Keys.Down: 
                    if (currentDirection != Direction.Up) nextDirection = Direction.Down; 
                    break;
                case Keys.Left: 
                    if (currentDirection != Direction.Right) nextDirection = Direction.Left; 
                    break;
                case Keys.Right: 
                    if (currentDirection != Direction.Left) nextDirection = Direction.Right; 
                    break;
            }
        }

        // --- Rendering / Menggambar ---
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // 1. Gambar Ular
            for (int i = 0; i < snake.Count; i++)
            {
                // Kepala beda warna dikit biar keren
                Brush snakeBrush = (i == 0) ? Brushes.Lime : Brushes.Green;
                g.FillRectangle(snakeBrush, 
                    snake[i].X * TileSize, 
                    snake[i].Y * TileSize, 
                    TileSize - 1, TileSize - 1); // -1 biar ada efek grid pixel
            }

            // 2. Gambar Makanan
            g.FillRectangle(Brushes.Red, 
                food.X * TileSize, 
                food.Y * TileSize, 
                TileSize - 1, TileSize - 1);

            // 3. Gambar Obstacles (Rintangan)
            foreach (var obs in obstacles)
            {
                g.FillRectangle(Brushes.Gray, 
                    obs.X * TileSize, 
                    obs.Y * TileSize, 
                    TileSize - 1, TileSize - 1);

                // Efek silang di obstacle
                g.DrawLine(Pens.Black, 
                    obs.X * TileSize, obs.Y * TileSize, 
                    (obs.X + 1) * TileSize, (obs.Y + 1) * TileSize);
            }

            // 4. GUI Score
            string status = $"Score: {score} | Level: {level}";
            g.DrawString(status, new Font("Consolas", 12, FontStyle.Bold), Brushes.White, 5, this.ClientSize.Height - 30);
            
            // Garis pembatas status bar
            g.DrawLine(Pens.White, 0, this.ClientSize.Height - 35, this.ClientSize.Width, this.ClientSize.Height - 35);
        }
        
        // Designer boilerplate
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }
    }
}