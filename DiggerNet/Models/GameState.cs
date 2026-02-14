using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using DiggerNet.Audio;

namespace DiggerNet.Models
{
    public class GameState
    {
        public int Width { get; set; } = 15;
        public int Height { get; set; } = 10;
        public int TileSize { get; set; } = 32;

        public TileType[,] Map { get; set; } = new TileType[0,0];
        public Digger Player { get; set; } = new Digger();
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();
        public List<Projectile> Projectiles { get; set; } = new List<Projectile>();
        public List<Tuple<int, int>> GoldBags { get; set; } = new List<Tuple<int, int>>();
        
        public int Score { get; set; }
        public int Lives { get; set; } = 3;
        public int Level { get; set; } = 1;
        public bool IsPaused { get; set; } = true;
        public bool IsGameOver { get; set; }

        private DispatcherTimer _gameTimer;
        private Random _rand = new Random();

        public double GameSpeedMultiplier { get; set; } = 1.0;
        private const double BaseTickRate = 100; // ms

        public event EventHandler? RequestRender; 

        public GameState()
        {
            InitializeLevel();
            _gameTimer = new DispatcherTimer();
            _gameTimer.Tick += GameTick;
            _gameTimer.Interval = TimeSpan.FromMilliseconds(BaseTickRate);
        }

        public void Start()
        {
            if (IsGameOver)
            {
                Score = 0;
                Lives = 3;
                Level = 1;
                IsGameOver = false;
                InitializeLevel();
            }
            IsPaused = false;
            _gameTimer.Start();
        }

        public void NewGame()
        {
            Score = 0;
            Lives = 3;
            Level = 1;
            IsGameOver = false;
            IsPaused = false;
            InitializeLevel();
            _gameTimer.Start();
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
            if (IsPaused) _gameTimer.Stop();
            else _gameTimer.Start();
        }

        private void InitializeLevel()
        {
            Map = GenerateMap();
            Player = new Digger { X = 1, Y = 1 };
            Enemies = new List<Enemy>();
            Projectiles = new List<Projectile>();
            GoldBags = new List<Tuple<int, int>>();

            for (int i = 0; i < Level + 2; i++)
            {
                SpawnEnemy();
            }

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (Map[x, y] == TileType.GoldBag)
                    {
                        GoldBags.Add(new Tuple<int, int>(x, y));
                    }
                }
            }
        }

        private TileType[,] GenerateMap()
        {
            var map = new TileType[Width, Height];
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                    {
                        map[x, y] = TileType.Wall;
                    }
                    else
                    {
                        map[x, y] = TileType.Dirt;
                        if (_rand.Next(10) == 0) map[x, y] = TileType.Emerald;
                        else if (_rand.Next(30) == 0) map[x, y] = TileType.GoldBag;
                    }
                }
            }
            map[1, 1] = TileType.Empty; 
            return map;
        }

        private void SpawnEnemy()
        {
            Enemies.Add(new Enemy 
            { 
                X = Width - 2, 
                Y = 1, 
                SubType = Enemy.EnemyType.Nobbin 
            });
        }

        private void GameTick(object? sender, EventArgs e)
        {
            if (IsPaused || IsGameOver) return;

            UpdatePlayer();
            UpdateProjectiles();
            UpdateEnemies();
            UpdateGoldBags();
            CheckCollisions();

            RequestRender?.Invoke(this, EventArgs.Empty);
        }

        public void MovePlayer(Direction dir)
        {
            if (IsPaused || IsGameOver) return;
            
            int newX = Player.X;
            int newY = Player.Y;

            switch (dir)
            {
                case Direction.Up: newY--; break;
                case Direction.Down: newY++; break;
                case Direction.Left: newX--; break;
                case Direction.Right: newX++; break;
            }

            if (newX < 0 || newX >= Width || newY < 0 || newY >= Height) return;

            Player.CurrentDirection = dir;

            if (Map[newX, newY] == TileType.Wall) return;

            if (Map[newX, newY] == TileType.GoldBag)
            {
                int behindX = newX + (newX - Player.X);
                int behindY = newY + (newY - Player.Y);
                
                if (behindX >= 0 && behindX < Width && behindY >= 0 && behindY < Height && 
                   Map[behindX, behindY] == TileType.Empty)
                {
                    Map[newX, newY] = TileType.Empty;
                    Map[behindX, behindY] = TileType.GoldBag;
                    
                    var oldPos = GoldBags.FirstOrDefault(g => g.Item1 == newX && g.Item2 == newY);
                    if (oldPos != null)
                    {
                        GoldBags.Remove(oldPos);
                        GoldBags.Add(new Tuple<int, int>(behindX, behindY));
                    }
                    
                    Player.X = newX;
                    Player.Y = newY;
                }
                return; 
            }

            Player.X = newX;
            Player.Y = newY;

            if (Map[newX, newY] == TileType.Dirt)
            {
                Map[newX, newY] = TileType.Empty;
                SoundManager.PlayDig();
            }
            else if (Map[newX, newY] == TileType.Emerald)
            {
                Map[newX, newY] = TileType.Empty;
                Score += 25;
                SoundManager.PlayCollect();
            }
            else if (Map[newX, newY] == TileType.OpenGoldBag)
            {
                Map[newX, newY] = TileType.Empty;
                Score += 500;
                SoundManager.PlayGold();
            }
        }

        public void Fire()
        {
            if (Player.CurrentDirection == Direction.None) return;
            Projectiles.Add(new Projectile 
            { 
                X = Player.X, 
                Y = Player.Y, 
                FiredDirection = Player.CurrentDirection,
                IsAlive = true
            });
            SoundManager.PlayShoot();
        }

        private void UpdatePlayer() { }

        private void UpdateProjectiles()
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                if (!p.IsAlive)
                {
                    Projectiles.RemoveAt(i);
                    continue;
                }

                switch (p.FiredDirection)
                {
                    case Direction.Up: p.Y--; break;
                    case Direction.Down: p.Y++; break;
                    case Direction.Left: p.X--; break;
                    case Direction.Right: p.X++; break;
                }
                p.DistanceTraveled++;

                if (p.X < 0 || p.X >= Width || p.Y < 0 || p.Y >= Height || Map[p.X, p.Y] == TileType.Wall)
                {
                    p.IsAlive = false;
                }
                else if (Map[p.X, p.Y] == TileType.GoldBag)
                {
                    p.IsAlive = false;
                }
                else if (p.DistanceTraveled > 4) 
                {
                    p.IsAlive = false;
                }
            }
        }

        private void UpdateEnemies()
        {
            foreach (var enemy in Enemies)
            {
                if (!enemy.IsAlive) continue;

                if (enemy.SubType == Enemy.EnemyType.Nobbin)
                {
                    enemy.TransformTimer++;
                    if (enemy.TransformTimer > 50) enemy.SubType = Enemy.EnemyType.Hobbin;
                }

                int dx = Player.X - enemy.X;
                int dy = Player.Y - enemy.Y;
                
                int nextX = enemy.X;
                int nextY = enemy.Y;

                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    nextX += Math.Sign(dx);
                }
                else
                {
                    nextY += Math.Sign(dy);
                }

                if (IsValidMove(enemy, nextX, nextY))
                {
                    enemy.X = nextX;
                    enemy.Y = nextY;
                }
                else
                {
                    if (nextX != enemy.X)
                    {
                        nextX = enemy.X;
                        nextY = enemy.Y + Math.Sign(dy);
                        if (IsValidMove(enemy, nextX, nextY)) enemy.Y = nextY;
                    }
                    else
                    {
                        nextY = enemy.Y;
                        nextX = enemy.X + Math.Sign(dx);
                        if (IsValidMove(enemy, nextX, nextY)) enemy.X = nextX;
                    }
                }
            }
        }

        private bool IsValidMove(Enemy enemy, int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return false;
            
            if (Map[x, y] == TileType.Wall) return false;
            if (Map[x, y] == TileType.GoldBag) return false;
            if (Map[x, y] == TileType.Dirt && enemy.SubType == Enemy.EnemyType.Nobbin) return false;
            
            if (Map[x, y] == TileType.Dirt && enemy.SubType == Enemy.EnemyType.Hobbin)
            {
                Map[x, y] = TileType.Empty;
            }

            return true;
        }

        private void UpdateGoldBags()
        {
            var bags = GoldBags.ToList(); 
            foreach (var bag in bags)
            {
                int x = bag.Item1;
                int y = bag.Item2;

                if (y + 1 < Height && Map[x, y + 1] == TileType.Empty)
                {
                    Map[x, y] = TileType.Empty;
                    Map[x, y + 1] = TileType.GoldBag;
                    
                    GoldBags.Remove(bag);
                    GoldBags.Add(new Tuple<int, int>(x, y + 1));

                    var enemyHit = Enemies.FirstOrDefault(e => e.X == x && e.Y == y + 1);
                    if (enemyHit != null)
                    {
                        enemyHit.IsAlive = false;
                        Score += 250;
                        Enemies.Remove(enemyHit);
                        SoundManager.PlayDie(); // Enemy die sound
                    }

                    if (Player.X == x && Player.Y == y + 1)
                    {
                        PlayerDies();
                    }
                    
                    if (y + 2 >= Height || Map[x, y+2] != TileType.Empty)
                    {
                         Map[x, y+1] = TileType.OpenGoldBag;
                         GoldBags.Remove(new Tuple<int, int>(x, y + 1));
                    }
                }
            }
        }

        private void CheckCollisions()
        {
            foreach (var enemy in Enemies)
            {
                if (enemy.X == Player.X && enemy.Y == Player.Y)
                {
                    PlayerDies();
                    return;
                }
            }

            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                var p = Projectiles[i];
                if (!p.IsAlive) continue;

                var hitEnemy = Enemies.FirstOrDefault(e => e.X == p.X && e.Y == p.Y);
                if (hitEnemy != null)
                {
                    hitEnemy.IsAlive = false;
                    p.IsAlive = false;
                    Score += 200;
                    Enemies.Remove(hitEnemy);
                    SoundManager.PlayDie();
                }
            }
            
            bool emeraldsLeft = false;
            foreach (var t in Map) if (t == TileType.Emerald) emeraldsLeft = true;
            
            if (!emeraldsLeft && Enemies.Count == 0)
            {
                LevelUp();
            }
        }

        private void PlayerDies()
        {
            Lives--;
            SoundManager.PlayDie();
            if (Lives <= 0)
            {
                IsGameOver = true;
                _gameTimer.Stop();
            }
            else
            {
                Player.X = 1; 
                Player.Y = 1;
                Enemies.Clear();
                for(int i=0; i<Level+2; i++) SpawnEnemy();
            }
        }

        private void LevelUp()
        {
            Level++;
            Score += 1000;
            InitializeLevel();
        }
    }
}