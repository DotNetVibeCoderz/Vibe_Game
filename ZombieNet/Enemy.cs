using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ZombieNet
{
    public class Enemy
    {
        public Vector2 Position { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public float Speed { get; private set; }
        public bool IsDead => Health <= 0;
        public bool ReachedEnd { get; private set; }
        public int Bounty { get; private set; }

        private List<Vector2> _path;
        private int _currentPathIndex;
        private Texture2D _texture;

        public Enemy(Texture2D texture, List<Vector2> path, int health, float speed, int bounty)
        {
            _texture = texture;
            _path = path;
            Health = health;
            MaxHealth = health;
            Speed = speed;
            Bounty = bounty;

            if (path != null && path.Count > 0)
            {
                Position = path[0];
                _currentPathIndex = 0;
            }
        }

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }

        public void Update(GameTime gameTime)
        {
            if (IsDead || ReachedEnd || _path == null || _currentPathIndex >= _path.Count - 1)
                return;

            Vector2 target = _path[_currentPathIndex + 1];
            Vector2 direction = target - Position;
            float distance = direction.Length();

            if (distance < Speed)
            {
                Position = target;
                _currentPathIndex++;
                if (_currentPathIndex >= _path.Count - 1)
                {
                    ReachedEnd = true;
                }
            }
            else
            {
                direction.Normalize();
                Position += direction * Speed;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsDead && !ReachedEnd)
            {
                // Draw centered
                Vector2 origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
                spriteBatch.Draw(_texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
                
                // Simple health bar
                float healthPct = (float)Health / MaxHealth;
                spriteBatch.Draw(_texture, new Rectangle((int)Position.X - 16, (int)Position.Y - 24, 32, 4), Color.Red);
                spriteBatch.Draw(_texture, new Rectangle((int)Position.X - 16, (int)Position.Y - 24, (int)(32 * healthPct), 4), Color.Green);
            }
        }
    }
}
