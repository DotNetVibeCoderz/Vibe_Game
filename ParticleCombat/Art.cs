using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ParticleCombat
{
    public static class Art
    {
        public static Texture2D Pixel { get; private set; }
        public static Texture2D Circle { get; private set; }
        public static Texture2D Star { get; private set; }
        public static Texture2D Ship { get; private set; }
        public static Texture2D Alien { get; private set; }

        public static void Load(GraphicsDevice device)
        {
            Pixel = new Texture2D(device, 1, 1);
            Pixel.SetData(new[] { Color.White });

            Circle = CreateCircle(device, 16);
            Star = CreateStar(device, 32);
            Ship = CreateShip(device, 30);
            Alien = CreateAlien(device, 30);
        }

        private static Texture2D CreateCircle(GraphicsDevice device, int radius)
        {
            int diameter = radius * 2;
            Texture2D texture = new Texture2D(device, diameter, diameter);
            Color[] data = new Color[diameter * diameter];

            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    int index = x + y * diameter;
                    Vector2 pos = new Vector2(x - radius, y - radius);
                    if (pos.Length() <= radius)
                    {
                        data[index] = Color.White;
                    }
                    else
                    {
                        data[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }

        private static Texture2D CreateStar(GraphicsDevice device, int size)
        {
            Texture2D texture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            Vector2 center = new Vector2(size / 2f);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int index = x + y * size;
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);
                    
                    if (dist < size / 2 && (Math.Abs(pos.X - center.X) < 2 || Math.Abs(pos.Y - center.Y) < 2))
                    {
                        data[index] = Color.White;
                    }
                    else
                    {
                        data[index] = Color.Transparent;
                    }
                }
            }
            texture.SetData(data);
            return texture;
        }

        private static Texture2D CreateShip(GraphicsDevice device, int size)
        {
            // Create a triangle/fighter shape pointing RIGHT (for 0 rotation)
            Texture2D texture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            Vector2 center = new Vector2(size / 2f);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int index = x + y * size;
                    
                    // Simple logic for a sleek spaceship
                    // Tip at (size, center.Y)
                    // Back at (0, 0) and (0, size)
                    
                    float dx = x - center.X;
                    float dy = y - center.Y;

                    // Check if point is inside a triangle
                    // Vertices: (size, size/2), (0, 0), (0, size)
                    // Actually let's make it a bit cooler, indented back
                    
                    bool inside = false;
                    
                    // Main body
                    if (x > 0 && Math.Abs(dy) < (x / 2.0f)) inside = true;
                    
                    // Engine cutouts
                    if (x < size / 4 && Math.Abs(dy) < size / 6) inside = false;

                    // Cockpit
                    if (x > size / 3 && x < size / 1.5 && Math.Abs(dy) < size / 6)
                    {
                         data[index] = Color.Cyan; // Cockpit glass
                         continue;
                    }

                    if (inside)
                    {
                        data[index] = Color.White;
                    }
                    else
                    {
                        data[index] = Color.Transparent;
                    }
                }
            }
            texture.SetData(data);
            return texture;
        }

        private static Texture2D CreateAlien(GraphicsDevice device, int size)
        {
            // Create a Space Invader / Bug shape
            Texture2D texture = new Texture2D(device, size, size);
            Color[] data = new Color[size * size];
            
            // 11x8 grid is standard invader, let's map it to size
            int[,] shape = new int[,] {
                {0,0,1,0,0,0,0,0,1,0,0},
                {0,0,0,1,0,0,0,1,0,0,0},
                {0,0,1,1,1,1,1,1,1,0,0},
                {0,1,1,0,1,1,1,0,1,1,0}, // Eyes
                {1,1,1,1,1,1,1,1,1,1,1},
                {1,0,1,1,1,1,1,1,1,0,1},
                {1,0,1,0,0,0,0,0,1,0,1},
                {0,0,0,1,1,0,1,1,0,0,0}
            };
            
            int gridW = 11;
            int gridH = 8;
            
            float cellW = size / (float)gridW;
            float cellH = size / (float)gridH;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int index = x + y * size;
                    int gx = (int)(x / cellW);
                    int gy = (int)(y / cellH);
                    
                    if(gx >= 0 && gx < gridW && gy >= 0 && gy < gridH && shape[gy, gx] == 1)
                    {
                        data[index] = Color.White;
                    }
                    else
                    {
                        data[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(data);
            return texture;
        }
    }
}