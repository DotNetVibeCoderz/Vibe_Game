using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ParticleCombat
{
    public abstract class Entity
    {
        protected Texture2D image;
        protected Color color = Color.White;

        public Vector2 Position, Velocity;
        public float Orientation;
        public float Radius = 20; 
        public bool IsExpired;
        public float Scale = 1f;

        public Vector2 Size
        {
            get
            {
                return image == null ? Vector2.Zero : new Vector2(image.Width, image.Height);
            }
        }

        public abstract void Update();

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (image != null)
                spriteBatch.Draw(image, Position, null, color, Orientation, Size / 2f, Scale, SpriteEffects.None, 0);
        }
    }

    public class Player : Entity
    {
        private static Player instance;
        public static Player Instance
        {
            get
            {
                if (instance == null)
                    instance = new Player();

                return instance;
            }
        }

        public int CooldownRemaining = 0;
        public int CooldownFrames = 6;
        private Random rand = new Random();

        private Player()
        {
            Reset();
        }

        public void Reset()
        {
            if(Art.Ship != null) 
                image = Art.Ship;
            else if (Art.Circle != null)
                image = Art.Circle;
            
            Position = new Vector2(Game1.ScreenSize.X / 2, Game1.ScreenSize.Y / 2);
            Velocity = Vector2.Zero;
            IsExpired = false;
            CooldownRemaining = 0;
            Radius = 15; // Slightly larger for ship
        }

        public override void Update()
        {
            if (image == null) 
            {
                if(Art.Ship != null) image = Art.Ship;
                else if (Art.Circle != null) image = Art.Circle;
            }
            
            if (IsExpired) return;

            Velocity = Vector2.Zero;

            if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W)) Velocity.Y -= 1;
            if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S)) Velocity.Y += 1;
            if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A)) Velocity.X -= 1;
            if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D)) Velocity.X += 1;

            if (Velocity != Vector2.Zero)
                Velocity.Normalize();

            Position += Velocity * 5; 
            Position = Vector2.Clamp(Position, Size / 2, new Vector2(Game1.ScreenSize.X, Game1.ScreenSize.Y) - Size / 2);

            var aim = Input.MousePosition - Position;
            if (aim != Vector2.Zero)
            {
                Orientation = (float)Math.Atan2(aim.Y, aim.X);
            }

            if (Input.IsLeftMouseButtonDown() && CooldownRemaining <= 0)
            {
                float aimAngle = Orientation;
                Quaternion aimQuat = Quaternion.CreateFromYawPitchRoll(0, 0, aimAngle);

                float randomSpread = rand.NextFloat(-0.04f, 0.04f) + rand.NextFloat(-0.04f, 0.04f);
                Vector2 vel = MathUtil.FromPolar(aimAngle + randomSpread, 11f);

                // Adjust offset for ship shape
                Vector2 offset = Vector2.Transform(new Vector2(25, -8), aimQuat);
                EntityManager.Add(new Bullet(Position + offset, vel));

                offset = Vector2.Transform(new Vector2(25, 8), aimQuat);
                EntityManager.Add(new Bullet(Position + offset, vel));

                SoundSystem.Play(SoundSystem.Shoot, 0.5f, rand.NextFloat(-0.1f, 0.1f));
                CooldownRemaining = CooldownFrames;
            }

            if (CooldownRemaining > 0)
                CooldownRemaining--;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsExpired)
                base.Draw(spriteBatch);
        }

        public void Kill()
        {
            IsExpired = true;
        }
    }

    public class Enemy : Entity
    {
        private Random rand = new Random();
        public int PointValue { get; private set; }

        public Enemy(Texture2D image, Vector2 position)
        {
            this.image = image;
            Position = position;
            Radius = image.Width / 2f;
            // Aliens are usually green or purple
            color = Color.Lerp(Color.LimeGreen, Color.Purple, (float)rand.NextDouble());
            PointValue = 10;
        }

        public override void Update()
        {
            if (Player.Instance.IsExpired) return;

            var direction = Player.Instance.Position - Position;
            if(direction != Vector2.Zero)
                direction.Normalize();

            Velocity += direction * 0.15f; 

            if (Velocity.LengthSquared() > 9) 
            {
                Velocity.Normalize();
                Velocity *= 3;
            }
            
            Position += Velocity;
            
            // Orient towards player
            if (Velocity != Vector2.Zero)
                 Orientation = (float)Math.Atan2(Velocity.Y, Velocity.X);


            if (!Game1.Viewport.Contains(Position.ToPoint()))
            {
                if (Position.X < -50 || Position.X > Game1.ScreenSize.X + 50 || 
                    Position.Y < -50 || Position.Y > Game1.ScreenSize.Y + 50)
                {
                   IsExpired = true; 
                }
            }
        }

        public void WasShot()
        {
            IsExpired = true;
            for (int i = 0; i < 15; i++)
                 ParticleManager.CreateParticle(Position, Color.LimeGreen); // Alien blood
        }
    }

    public class Bullet : Entity
    {
        public Bullet(Vector2 position, Vector2 velocity)
        {
            image = Art.Pixel; 
            Position = position;
            Velocity = velocity;
            Orientation = (float)Math.Atan2(Velocity.Y, Velocity.X);
            Radius = 8;
            color = Color.Gold;
        }

        public override void Update()
        {
            if (Velocity.LengthSquared() > 0)
                Orientation = (float)Math.Atan2(Velocity.Y, Velocity.X);

            Position += Velocity;

            if (!Game1.Viewport.Contains(Position.ToPoint()))
                IsExpired = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
             if(image != null)
                spriteBatch.Draw(image, Position, null, color, Orientation, new Vector2(0.5f, 0.5f), new Vector2(20, 3), SpriteEffects.None, 0);
        }
    }

    public static class EntityManager
    {
        static List<Entity> entities = new List<Entity>();
        static List<Entity> addedEntities = new List<Entity>();
        public static int Count { get { return entities.Count; } }
        static Random rand = new Random();

        public static void Add(Entity entity)
        {
            addedEntities.Add(entity);
        }

        public static void Update()
        {
            foreach (var entity in entities)
                entity.Update();

            entities.AddRange(addedEntities);
            addedEntities.Clear();

            var bullets = entities.OfType<Bullet>().ToList();
            var enemies = entities.OfType<Enemy>().ToList();

            foreach(var bullet in bullets)
            {
                foreach(var enemy in enemies)
                {
                    if(!enemy.IsExpired && !bullet.IsExpired && 
                       Vector2.DistanceSquared(bullet.Position, enemy.Position) < (bullet.Radius + enemy.Radius) * (bullet.Radius + enemy.Radius))
                    {
                        enemy.WasShot();
                        bullet.IsExpired = true;
                        Game1.Score += enemy.PointValue;
                        SoundSystem.Play(SoundSystem.Explosion, 0.5f, rand.NextFloat(-0.2f, 0.2f));
                    }
                }
            }

            if (!Player.Instance.IsExpired)
            {
                Player.Instance.Update(); 
                foreach (var enemy in enemies)
                {
                    if (!enemy.IsExpired && 
                        Vector2.DistanceSquared(enemy.Position, Player.Instance.Position) < (enemy.Radius + Player.Instance.Radius) * (enemy.Radius + Player.Instance.Radius))
                    {
                        Player.Instance.Kill();
                        enemy.IsExpired = true;
                        SoundSystem.Play(SoundSystem.Explosion, 1.0f, -0.5f);
                        for (int i = 0; i < 50; i++)
                            ParticleManager.CreateParticle(Player.Instance.Position, Color.Orange);
                    }
                }
            }

            entities = entities.Where(x => !x.IsExpired).ToList();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if(!Player.Instance.IsExpired)
                Player.Instance.Draw(spriteBatch);

            foreach (var entity in entities)
                entity.Draw(spriteBatch);
        }

        public static void Clear()
        {
            entities.Clear();
            addedEntities.Clear();
            Player.Instance.Reset();
        }
    }

    public static class MathUtil
    {
        public static Vector2 FromPolar(float angle, float magnitude)
        {
            return magnitude * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }
        
        public static float NextFloat(this Random rand, float minValue, float maxValue)
        {
            return (float)rand.NextDouble() * (maxValue - minValue) + minValue;
        }
    }
}