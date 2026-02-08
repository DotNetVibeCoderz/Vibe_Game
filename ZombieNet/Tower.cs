using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ZombieNet
{
    public class Tower
    {
        public Vector2 Position { get; private set; }
        public float Range { get; private set; }
        public int Damage { get; private set; }
        public float FireRate { get; private set; }
        public float _fireTimer;

        private Texture2D _texture;
        private Texture2D _bulletTexture;

        public Tower(Texture2D texture, Texture2D bulletTexture, Vector2 position, float range, int damage, float fireRate)
        {
            _texture = texture;
            _bulletTexture = bulletTexture;
            Position = position;
            Range = range;
            Damage = damage;
            FireRate = fireRate;
            _fireTimer = 0;
        }

        public void Update(GameTime gameTime, List<Enemy> enemies, List<Bullet> bullets)
        {
            _fireTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_fireTimer >= FireRate)
            {
                // Find nearest enemy
                Enemy target = null;
                float closestDist = Range;

                foreach (var enemy in enemies)
                {
                    if (enemy.IsDead) continue;

                    float dist = Vector2.Distance(Position, enemy.Position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        target = enemy;
                    }
                }

                if (target != null)
                {
                    // Fire
                    bullets.Add(new Bullet(_bulletTexture, Position, target, Damage));
                    _fireTimer = 0;
                    
                    // Play sound
                    SoundManager.PlayShoot();
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
            spriteBatch.Draw(_texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
        }
    }
}
