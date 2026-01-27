using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SpaceShooter
{
    public abstract class GameObject
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Speed { get; set; }
        public bool IsActive { get; set; } = true;
        protected Image? Sprite { get; set; }

        public Rectangle Bounds => new Rectangle((int)X, (int)Y, Width, Height);

        public abstract void Update(float deltaTime);
        public virtual void Draw(Graphics g)
        {
            if (Sprite != null)
                g.DrawImage(Sprite, X, Y, Width, Height);
            else
                g.FillRectangle(Brushes.Magenta, X, Y, Width, Height); // Fallback
        }
    }

    public class Player : GameObject
    {
        public int Health { get; set; } = 100;
        public int Score { get; set; } = 0;
        public DateTime LastShot { get; set; }

        public Player(int x, int y)
        {
            X = x; Y = y;
            Width = 50; Height = 50;
            Speed = 350f;
            try { if (System.IO.File.Exists("Assets/player.png")) Sprite = Image.FromFile("Assets/player.png"); } catch {}
        }

        public override void Update(float deltaTime) { }

        public override void Draw(Graphics g)
        {
            Random rnd = new Random();
            g.FillEllipse(Brushes.OrangeRed, X + Width/2 - 8, Y + Height - 10, 16, 15 + rnd.Next(15));
            g.FillEllipse(Brushes.Yellow, X + Width/2 - 4, Y + Height - 5, 8, 10 + rnd.Next(10));
            base.Draw(g);
        }
    }

    public enum EnemyType { Scout, Fighter, Boss }

    public class Enemy : GameObject
    {
        public EnemyType Type { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        private float _time;

        public Enemy(float x, float y, EnemyType type)
        {
            X = x; Y = y;
            Type = type;
            _time = (float)(new Random().NextDouble() * Math.PI * 2);

            switch (type)
            {
                case EnemyType.Scout:
                    Width = 40; Height = 40; Speed = 180f; Health = 1;
                    try { if (System.IO.File.Exists("Assets/scout.png")) Sprite = Image.FromFile("Assets/scout.png"); } catch {}
                    break;
                case EnemyType.Fighter:
                    Width = 50; Height = 50; Speed = 130f; Health = 3;
                    try { if (System.IO.File.Exists("Assets/fighter.png")) Sprite = Image.FromFile("Assets/fighter.png"); } catch {}
                    break;
                case EnemyType.Boss:
                    Width = 200; Height = 130; Speed = 40f; Health = 50;
                    try { if (System.IO.File.Exists("Assets/boss.png")) Sprite = Image.FromFile("Assets/boss.png"); } catch {}
                    break;
            }
            MaxHealth = Health;
        }

        public override void Update(float deltaTime)
        {
            _time += deltaTime;
            Y += Speed * deltaTime;

            if (Type == EnemyType.Fighter) X += (float)Math.Sin(_time * 3) * 3;
            if (Type == EnemyType.Boss)
            {
                X += (float)Math.Cos(_time) * 5;
                if (Y > 80) Y = 80;
            }
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);
            if (Type == EnemyType.Boss)
            {
                g.FillRectangle(Brushes.DarkRed, X, Y - 20, Width, 10);
                g.FillRectangle(Brushes.Lime, X, Y - 20, Width * ((float)Health / MaxHealth), 10);
                g.DrawRectangle(Pens.White, X, Y - 20, Width, 10);
            }
        }
    }

    public class Bullet : GameObject
    {
        public bool FromPlayer { get; set; }
        public Bullet(float x, float y, bool fromPlayer)
        {
            X = x; Y = y;
            Width = 6; Height = 20;
            Speed = fromPlayer ? -650f : 350f;
            FromPlayer = fromPlayer;
        }
        public override void Update(float deltaTime) { Y += Speed * deltaTime; }
        public override void Draw(Graphics g)
        {
            Color c = FromPlayer ? Color.Cyan : Color.OrangeRed;
            using (Brush b = new SolidBrush(c))
            {
                g.FillEllipse(b, X, Y, Width, Height);
            }
        }
    }

    public class Star : GameObject
    {
        public float Size { get; set; }
        public Star(Random rnd, int sw, int sh)
        {
            X = rnd.Next(sw); Y = rnd.Next(sh);
            Size = rnd.Next(1, 4);
            Speed = rnd.Next(50, 150);
        }
        public override void Update(float deltaTime) { Y += Speed * deltaTime; }
        public override void Draw(Graphics g) { g.FillEllipse(Brushes.White, X, Y, Size, Size); }
    }

    public class Planet : GameObject
    {
        public Color PlanetColor { get; set; }
        public Planet(Random rnd, int sw, int sh)
        {
            X = rnd.Next(sw); Y = -300;
            Width = rnd.Next(150, 300); Height = Width;
            Speed = rnd.Next(15, 35);
            PlanetColor = Color.FromArgb(rnd.Next(60, 120), rnd.Next(60, 120), rnd.Next(100, 200));
        }
        public override void Update(float deltaTime) { Y += Speed * deltaTime; }
        public override void Draw(Graphics g)
        {
            using (var b = new SolidBrush(Color.FromArgb(100, PlanetColor)))
                g.FillEllipse(b, X, Y, Width, Height);
        }
    }
}
