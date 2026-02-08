using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ZombieNet
{
    public class Map
    {
        public Tile[,] Tiles { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public List<Vector2> Path { get; private set; }
        public Vector2 SpawnPoint { get; private set; }
        public Vector2 BasePoint { get; private set; }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            Path = new List<Vector2>();

            // Initialize all as grass
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y] = new Tile(x, y, TileType.Grass);
                }
            }

            GeneratePath();
        }

        private void GeneratePath()
        {
            // Create a path for 20x20 map or larger
            if (Width >= 20 && Height >= 20)
            {
                SpawnPoint = new Vector2(0, 2);
                BasePoint = new Vector2(Width - 1, Height - 2);

                AddPathSegment(0, 2, 15, 2);
                AddPathSegment(15, 2, 15, 10);
                AddPathSegment(15, 10, 5, 10);
                AddPathSegment(5, 10, 5, 18);
                AddPathSegment(5, 18, Width - 1, 18);
            }
            else // Default small map logic
            {
                SpawnPoint = new Vector2(0, 2);
                BasePoint = new Vector2(Width - 1, Height - 2);

                AddPathSegment(0, 2, Width - 4, 2);
                AddPathSegment(Width - 4, 2, Width - 4, Height / 2);
                AddPathSegment(Width - 4, Height / 2, 2, Height / 2);
                AddPathSegment(2, Height / 2, 2, Height - 2);
                AddPathSegment(2, Height - 2, Width - 1, Height - 2);
            }

            // Mark spawn and base
            if (Path.Count > 0)
            {
                // Ensure spawn and base are correct types
                // Start of path
                Vector2 start = Path[0]; // Center pos
                int sx = (int)(start.X / Tile.Size);
                int sy = (int)(start.Y / Tile.Size);
                Tiles[sx, sy] = new Tile(sx, sy, TileType.Spawn);

                // End of path
                Vector2 end = Path[Path.Count - 1];
                int ex = (int)(end.X / Tile.Size);
                int ey = (int)(end.Y / Tile.Size);
                Tiles[ex, ey] = new Tile(ex, ey, TileType.Base);
            }
        }

        private void AddPathSegment(int startX, int startY, int endX, int endY)
        {
            // Simple horizontal or vertical line logic
            if (startX == endX) // Vertical
            {
                int step = endY > startY ? 1 : -1;
                // Inclusive range for path drawing, but need to be careful not to duplicate corners too much
                for (int y = startY; y != endY + step; y += step)
                {
                    if (y >= 0 && y < Height)
                    {
                        Tiles[startX, y] = new Tile(startX, y, TileType.Road);
                        Path.Add(new Vector2(startX * Tile.Size + Tile.Size / 2, y * Tile.Size + Tile.Size / 2));
                    }
                }
            }
            else // Horizontal
            {
                int step = endX > startX ? 1 : -1;
                for (int x = startX; x != endX + step; x += step)
                {
                    if (x >= 0 && x < Width)
                    {
                        Tiles[x, startY] = new Tile(x, startY, TileType.Road);
                        Path.Add(new Vector2(x * Tile.Size + Tile.Size / 2, startY * Tile.Size + Tile.Size / 2));
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D grassTexture, Texture2D roadTexture)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Tiles[x, y].Draw(spriteBatch, grassTexture, roadTexture);
                }
            }
        }
    }
}
