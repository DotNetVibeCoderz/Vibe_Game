using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace BattleTank
{
    public enum TileType { Empty, Brick, Steel, Water, Tree, Eagle }

    public class Map
    {
        public TileType[,] Grid;
        public int Width = 25;
        public int Height = 19; // 32 * 19 = 608 (approx 600)
        public int TileSize = 32;
        
        public List<Rectangle> Obstacles = new List<Rectangle>();

        public void Generate()
        {
            Grid = new TileType[Width, Height];
            System.Random rand = new System.Random();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Eagle Base
                    if (x == 12 && y == 18) { Grid[x, y] = TileType.Eagle; continue; }
                    if ((x == 11 || x == 13) && y == 18) { Grid[x, y] = TileType.Brick; continue; }
                    if ((x >= 11 && x <= 13) && y == 17) { Grid[x, y] = TileType.Brick; continue; }

                    // Random generation
                    int roll = rand.Next(100);
                    if (roll < 5) Grid[x, y] = TileType.Steel;
                    else if (roll < 20) Grid[x, y] = TileType.Brick;
                    else if (roll < 25) Grid[x, y] = TileType.Water;
                    else if (roll < 30) Grid[x, y] = TileType.Tree;
                    else Grid[x, y] = TileType.Empty;

                    // Clear spawn areas
                    if (y < 3 && (x < 3 || x > 21 || (x > 10 && x < 14))) Grid[x, y] = TileType.Empty;
                    if (y > 15 && x > 8 && x < 16) Grid[x, y] = TileType.Empty; // Clear area around base for player spawn
                }
            }
            Grid[12, 18] = TileType.Eagle; // Ensure Eagle
            UpdateObstacles();
        }

        public void UpdateObstacles()
        {
            Obstacles.Clear();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // Water is an obstacle for tanks but bullets fly over (logic handled elsewhere)
                    // For simple tank movement collision, we treat Brick, Steel, Water as obstacles
                    if (Grid[x, y] == TileType.Brick || Grid[x, y] == TileType.Steel || Grid[x, y] == TileType.Water || Grid[x, y] == TileType.Eagle)
                    {
                        Obstacles.Add(new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize));
                    }
                }
            }
        }

        public void DestroyTile(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                if (Grid[x, y] == TileType.Brick)
                {
                    Grid[x, y] = TileType.Empty;
                    UpdateObstacles();
                }
                else if (Grid[x, y] == TileType.Eagle)
                {
                    Grid[x, y] = TileType.Empty; // Game Over logic should trigger
                    UpdateObstacles();
                }
            }
        }

        public void Draw(SpriteBatch sb, Texture2D tBrick, Texture2D tSteel, Texture2D tWater, Texture2D tTree, Texture2D tEagle)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector2 pos = new Vector2(x * TileSize, y * TileSize);
                    switch (Grid[x, y])
                    {
                        case TileType.Brick: sb.Draw(tBrick, pos, Color.White); break;
                        case TileType.Steel: sb.Draw(tSteel, pos, Color.White); break;
                        case TileType.Water: sb.Draw(tWater, pos, Color.White); break;
                        case TileType.Tree: sb.Draw(tTree, pos, Color.White); break;
                        case TileType.Eagle: sb.Draw(tEagle, pos, Color.White); break;
                    }
                }
            }
        }
    }
}