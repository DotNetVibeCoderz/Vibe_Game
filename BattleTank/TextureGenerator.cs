using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BattleTank
{
    public static class TextureGenerator
    {
        public static Texture2D CreatePixelTexture(GraphicsDevice graphics, int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(graphics, width, height);
            Color[] data = new Color[width * height];
            for (int i = 0; i < data.Length; i++) data[i] = color;
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateTankTexture(GraphicsDevice graphics, Color bodyColor)
        {
            // Smaller 28x28 tank for easier movement in 32x32 grid
            int size = 28;
            Texture2D texture = new Texture2D(graphics, size, size);
            Color[] data = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Body (centered 20x20)
                    if (x >= 4 && x < 24 && y >= 4 && y < 24) data[y * size + x] = bodyColor;
                    // Tracks (left and right strips)
                    else if ((x < 5 || x >= 23) && (y % 4 != 0)) data[y * size + x] = Color.Gray;
                    // Cannon (centered top)
                    else if (x >= 12 && x < 16 && y < 14) data[y * size + x] = Color.DarkGray;
                    else data[y * size + x] = Color.Transparent;
                }
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateBrickTexture(GraphicsDevice graphics)
        {
            // 32x32 brick
            Texture2D texture = new Texture2D(graphics, 32, 32);
            Color[] data = new Color[32 * 32];
            Color brickColor = new Color(178, 34, 34);
            Color mortarColor = Color.LightGray;

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    bool isMortar = (y % 8 == 7) || (x % 16 == 0 && (y / 8) % 2 == 0) || (x % 16 == 8 && (y / 8) % 2 != 0);
                    data[y * 32 + x] = isMortar ? mortarColor : brickColor;
                }
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateSteelTexture(GraphicsDevice graphics)
        {
            Texture2D texture = new Texture2D(graphics, 32, 32);
            Color[] data = new Color[32 * 32];
            for (int i = 0; i < data.Length; i++) data[i] = Color.LightGray;
            // Add some shine
            for(int i = 0; i < 32; i++) 
            {
                data[i * 32 + i] = Color.White; // Diagonal
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateTreeTexture(GraphicsDevice graphics)
        {
            Texture2D texture = new Texture2D(graphics, 32, 32);
            Color[] data = new Color[32 * 32];
            for (int i = 0; i < data.Length; i++) 
            {
                if ((i % 32 + i / 32) % 2 == 0) data[i] = Color.Green;
                else data[i] = Color.DarkGreen;
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateWaterTexture(GraphicsDevice graphics)
        {
            Texture2D texture = new Texture2D(graphics, 32, 32);
            Color[] data = new Color[32 * 32];
            for (int i = 0; i < data.Length; i++) data[i] = Color.Blue;
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateEagleTexture(GraphicsDevice graphics)
        {
            Texture2D texture = new Texture2D(graphics, 32, 32);
            Color[] data = new Color[32 * 32];
            for (int i = 0; i < data.Length; i++)
            {
                // Simple yellow eagle icon
                int x = i % 32;
                int y = i / 32;
                if(x > 8 && x < 24 && y > 8 && y < 24) data[i] = Color.Gold;
                else data[i] = Color.Transparent;
            }
            texture.SetData(data);
            return texture;
        }

        public static Texture2D CreateBulletTexture(GraphicsDevice graphics)
        {
            Texture2D texture = new Texture2D(graphics, 8, 8);
            Color[] data = new Color[8 * 8];
            for (int i = 0; i < data.Length; i++) data[i] = Color.White;
            texture.SetData(data);
            return texture;
        }
    }
}