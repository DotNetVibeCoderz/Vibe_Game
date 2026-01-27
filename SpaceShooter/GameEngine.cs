using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpaceShooter
{
    public enum GameState { Menu, Playing, GameOver, About }

    public class GameEngine
    {
        public GameState State { get; set; } = GameState.Menu;
        public Player Player { get; private set; }
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        public List<Bullet> Bullets { get; private set; } = new List<Bullet>();
        public List<Star> Stars { get; private set; } = new List<Star>();
        public List<Planet> Planets { get; private set; } = new List<Planet>();
        
        public int Level { get; private set; } = 1;
        private float _spawnTimer = 0;
        private float _planetTimer = 0;
        private Random _rnd = new Random();
        private int _screenWidth, _screenHeight;
        
        private bool _isBossEncounter = false;
        private int _scoreAtLastBoss = 0;
        private const int SCORE_FOR_BOSS = 1500; 

        public GameEngine(int width, int height)
        {
            _screenWidth = width;
            _screenHeight = height;
            
            SpriteGenerator.GenerateAssets();

            Player = new Player(_screenWidth / 2 - 25, _screenHeight - 100);
            InitGame();
            for (int i = 0; i < 100; i++) Stars.Add(new Star(_rnd, _screenWidth, _screenHeight));
        }

        public void InitGame()
        {
            Player.Health = 100;
            Player.Score = 0;
            Player.X = _screenWidth / 2 - 25;
            Player.Y = _screenHeight - 100;
            Enemies.Clear();
            Bullets.Clear();
            Planets.Clear();
            Level = 1;
            _isBossEncounter = false;
            _scoreAtLastBoss = 0;
        }

        public void Update(float deltaTime, bool left, bool right, bool up, bool down, bool shoot)
        {
            foreach (var star in Stars)
            {
                star.Update(deltaTime);
                if (star.Y > _screenHeight) { star.Y = -5; star.X = _rnd.Next(_screenWidth); }
            }

            _planetTimer += deltaTime;
            if (_planetTimer > 20) { Planets.Add(new Planet(_rnd, _screenWidth, _screenHeight)); _planetTimer = 0; }
            foreach (var p in Planets.ToList()) { p.Update(deltaTime); if (p.Y > _screenHeight + 300) Planets.Remove(p); }

            if (State != GameState.Playing) return;

            if (left && Player.X > 0) Player.X -= Player.Speed * deltaTime;
            if (right && Player.X < _screenWidth - Player.Width) Player.X += Player.Speed * deltaTime;
            if (up && Player.Y > 0) Player.Y -= Player.Speed * deltaTime;
            if (down && Player.Y < _screenHeight - Player.Height) Player.Y += Player.Speed * deltaTime;

            if (shoot && (DateTime.Now - Player.LastShot).TotalMilliseconds > 180)
            {
                Bullets.Add(new Bullet(Player.X + Player.Width / 2 - 3, Player.Y, true));
                Player.LastShot = DateTime.Now;
                SoundManager.PlayShoot();
            }

            _spawnTimer += deltaTime;
            
            if (!_isBossEncounter && (Player.Score - _scoreAtLastBoss) >= SCORE_FOR_BOSS)
            {
                _isBossEncounter = true;
                var boss = new Enemy(_screenWidth / 2 - 100, -150, EnemyType.Boss);
                boss.Health = 40 + (Level * 30);
                boss.MaxHealth = boss.Health;
                Enemies.Add(boss);
                SoundManager.PlayBoss();
            }

            if (!_isBossEncounter)
            {
                if (_spawnTimer > Math.Max(0.2f, 1.4f - (Level * 0.15f)))
                {
                    EnemyType type = _rnd.Next(10) > (8 - Level) ? EnemyType.Fighter : EnemyType.Scout;
                    Enemies.Add(new Enemy(_rnd.Next(0, _screenWidth - 50), -50, type));
                    _spawnTimer = 0;
                }
            }
            else
            {
                foreach(var e in Enemies.Where(x => x.Type == EnemyType.Boss))
                {
                    if (_rnd.Next(100) < 4 + Level) 
                        Bullets.Add(new Bullet(e.X + _rnd.Next(e.Width), e.Y + e.Height, false));
                }
            }

            foreach (var e in Enemies.ToList())
            {
                e.Update(deltaTime);
                if (e.Y > _screenHeight + 150) Enemies.Remove(e);
                
                if (e.Bounds.IntersectsWith(Player.Bounds))
                {
                    Player.Health -= (e.Type == EnemyType.Boss ? 50 : 20);
                    Enemies.Remove(e);
                    SoundManager.PlayExplosion();
                    if (Player.Health <= 0) State = GameState.GameOver;
                }
            }

            foreach (var b in Bullets.ToList())
            {
                b.Update(deltaTime);
                if (b.Y < -50 || b.Y > _screenHeight + 50) Bullets.Remove(b);

                if (b.FromPlayer)
                {
                    foreach (var e in Enemies.ToList())
                    {
                        if (b.Bounds.IntersectsWith(e.Bounds))
                        {
                            e.Health--;
                            Bullets.Remove(b);
                            if (e.Health <= 0)
                            {
                                Player.Score += e.Type == EnemyType.Boss ? 2000 : 100;
                                Enemies.Remove(e);
                                SoundManager.PlayExplosion();
                                if (e.Type == EnemyType.Boss)
                                {
                                    Level++;
                                    _isBossEncounter = false;
                                    _scoreAtLastBoss = Player.Score; 
                                    Player.Health = Math.Min(100, Player.Health + 40); 
                                }
                            }
                            break;
                        }
                    }
                }
                else if (b.Bounds.IntersectsWith(Player.Bounds))
                {
                    Player.Health -= 8;
                    Bullets.Remove(b);
                    SoundManager.PlayExplosion();
                    if (Player.Health <= 0) State = GameState.GameOver;
                }
            }
        }

        public void Draw(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(Color.FromArgb(5, 5, 20));

            foreach (var p in Planets) p.Draw(g);
            foreach (var s in Stars) s.Draw(g);
            
            if (State == GameState.Playing || State == GameState.GameOver)
            {
                foreach (var b in Bullets) b.Draw(g);
                foreach (var e in Enemies) e.Draw(g);
                Player.Draw(g);

                using(Font f = new Font("Impact", 14))
                {
                    g.DrawString($"SCORE: {Player.Score:D6}", f, Brushes.White, 20, 20);
                    g.DrawString($"LEVEL: {Level}", f, Brushes.DeepSkyBlue, 20, 50);
                    
                    if (_isBossEncounter)
                        g.DrawString("WARNING: BOSS DETECTED!", f, Brushes.Red, _screenWidth/2 - 100, 20);
                    
                    g.DrawString($"HULL INTEGRITY", f, Brushes.White, _screenWidth - 170, 20);
                    g.FillRectangle(Brushes.DarkRed, _screenWidth - 170, 50, 150, 18);
                    g.FillRectangle(Brushes.Cyan, _screenWidth - 170, 50, (Player.Health / 100f) * 150, 18);
                    g.DrawRectangle(Pens.White, _screenWidth - 170, 50, 150, 18);
                }
            }

            if (State == GameState.Menu) DrawOverlay(g, "GALACTIC DEFENDER", "Press 'N' for New Mission\nPress 'A' for Intel\nPress 'X' to Abort");
            if (State == GameState.GameOver) DrawOverlay(g, "MISSION FAILED", $"Final Score: {Player.Score}\n\nPress ESC for Command Center");
            if (State == GameState.About) DrawOverlay(g, "INTEL REPORT", "Space Shooter v2.0\n\nSprites generated by Jacky's Core\nDeveloped at Gravicode Studios\n\nPress ESC to Return");
        }

        private void DrawOverlay(Graphics g, string title, string subtitle)
        {
            using (Brush b = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
                g.FillRectangle(b, 0, 0, _screenWidth, _screenHeight);

            using(Font titleFont = new Font("Impact", 56))
            using(Font subFont = new Font("Consolas", 16))
            {
                var titleSize = g.MeasureString(title, titleFont);
                g.DrawString(title, titleFont, Brushes.Cyan, (_screenWidth - titleSize.Width) / 2, _screenHeight / 2 - 140);
                
                var subSize = g.MeasureString(subtitle, subFont);
                g.DrawString(subtitle, subFont, Brushes.White, (_screenWidth - subSize.Width) / 2, _screenHeight / 2 + 40);
            }
        }
    }
}
