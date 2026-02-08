using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZombieNet
{
    public enum TileType
    {
        Grass,
        Road,
        Base,
        Spawn
    }

    public class Tile
    {
        public Vector2 Position { get; private set; }
        public TileType Type { get; private set; }
        public bool IsWalkable { get; private set; }
        public bool IsBuildable { get; private set; }
        public Rectangle Bounds { get; private set; }
        public const int Size = 64;

        public Tile(int x, int y, TileType type)
        {
            Position = new Vector2(x * Size, y * Size);
            Type = type;
            Bounds = new Rectangle((int)Position.X, (int)Position.Y, Size, Size);

            IsWalkable = type == TileType.Road || type == TileType.Spawn || type == TileType.Base;
            IsBuildable = type == TileType.Grass;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D grassTexture, Texture2D roadTexture)
        {
            Texture2D texture = Type == TileType.Grass ? grassTexture : roadTexture;
            // Additional logic for spawn/base could be added here
            spriteBatch.Draw(texture, Bounds, Color.White);
        }
    }
}
