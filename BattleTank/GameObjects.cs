using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BattleTank
{
    public enum Direction { Up, Down, Left, Right }

    public class Entity
    {
        public Vector2 Position;
        public Vector2 Size;
        public Texture2D Texture;
        public bool IsActive = true;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

        public Entity(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
            Size = new Vector2(texture.Width, texture.Height);
        }

        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
                spriteBatch.Draw(Texture, Position, Color.White);
        }
    }

    public class Bullet : Entity
    {
        public Direction Direction;
        public float Speed = 300f;
        public bool IsPlayerBullet;

        public Bullet(Vector2 position, Texture2D texture, Direction direction, bool isPlayerBullet) 
            : base(position, texture)
        {
            Direction = direction;
            IsPlayerBullet = isPlayerBullet;
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            switch (Direction)
            {
                case Direction.Up: Position.Y -= Speed * dt; break;
                case Direction.Down: Position.Y += Speed * dt; break;
                case Direction.Left: Position.X -= Speed * dt; break;
                case Direction.Right: Position.X += Speed * dt; break;
            }

            if (Position.X < 0 || Position.Y < 0 || Position.X > 800 || Position.Y > 640)
                IsActive = false;
        }
    }

    public class Tank : Entity
    {
        public Direction Direction;
        public float Speed = 100f;
        public float ShootCooldown = 0f;
        public int Lives = 1;
        public bool IsPlayer;

        public Tank(Vector2 position, Texture2D texture, bool isPlayer) : base(position, texture)
        {
            IsPlayer = isPlayer;
            if (isPlayer) Lives = 3;
        }

        public void Move(Direction dir, float dt, List<Rectangle> obstacles)
        {
            Direction = dir;
            Vector2 nextPos = Position;
            switch (dir)
            {
                case Direction.Up: nextPos.Y -= Speed * dt; break;
                case Direction.Down: nextPos.Y += Speed * dt; break;
                case Direction.Left: nextPos.X -= Speed * dt; break;
                case Direction.Right: nextPos.X += Speed * dt; break;
            }

            Rectangle nextRect = new Rectangle((int)nextPos.X, (int)nextPos.Y, (int)Size.X, (int)Size.Y);
            
            // Boundary check - Updated to 608 to match Map Height (19 * 32)
            // Using 608 allows movement in the bottom row.
            if (nextPos.X < 0 || nextPos.Y < 0 || nextPos.X + Size.X > 800 || nextPos.Y + Size.Y > 608)
                return;

            // Obstacle check
            foreach (var obs in obstacles)
            {
                if (nextRect.Intersects(obs))
                    return;
            }

            Position = nextPos;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive) return;

            float rotation = 0f;
            switch (Direction)
            {
                case Direction.Up: rotation = 0f; break;
                case Direction.Down: rotation = MathHelper.Pi; break;
                case Direction.Left: rotation = -MathHelper.PiOver2; break;
                case Direction.Right: rotation = MathHelper.PiOver2; break;
            }
            
            // Draw centered for rotation
            Vector2 origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
            Vector2 drawPos = Position + origin;

            spriteBatch.Draw(Texture, drawPos, null, IsPlayer ? Color.Yellow : Color.Red, rotation, origin, 1f, SpriteEffects.None, 0f);
        }
    }
}