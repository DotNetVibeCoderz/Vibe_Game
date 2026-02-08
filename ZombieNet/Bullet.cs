using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombieNet
{
    public class Bullet
    {
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public int Damage { get; private set; }
        public bool IsActive { get; private set; }
        public Enemy Target { get; private set; }

        private Texture2D _texture;

        public Bullet(Texture2D texture, Vector2 position, Enemy target, int damage)
        {
            _texture = texture;
            Position = position;
            Target = target;
            Damage = damage;
            IsActive = true;

            if (target != null)
            {
                Vector2 direction = target.Position - position;
                direction.Normalize();
                Velocity = direction * 10f; // Fast bullet
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!IsActive) return;

            // Homing or straight? Let's do simple homing for now to guarantee hit visually
            if (Target != null && !Target.IsDead)
            {
                Vector2 direction = Target.Position - Position;
                float distance = direction.Length();

                if (distance < 10f) // Hit
                {
                    Target.TakeDamage(Damage);
                    IsActive = false;
                    SoundManager.PlayHit();
                }
                else
                {
                    direction.Normalize();
                    Velocity = direction * 10f;
                    Position += Velocity;
                }
            }
            else
            {
                // Target dead or null, just keep flying straight or destroy
                Position += Velocity;
                // Add boundary check or lifetime to destroy
                if (Position.X < -100 || Position.X > 2000 || Position.Y < -100 || Position.Y > 2000)
                    IsActive = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                Vector2 origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
                spriteBatch.Draw(_texture, Position, null, Color.White, 0f, origin, 1f, SpriteEffects.None, 0f);
            }
        }
    }
}
