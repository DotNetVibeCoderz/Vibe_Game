using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Collections.Generic;

namespace CrossyGame.Utils
{
    public static class AssetManager
    {
        public static Dictionary<string, Bitmap> Assets { get; private set; } = new Dictionary<string, Bitmap>();

        public static void LoadAssets()
        {
            Assets["grass"] = CreateTile(32, 32, Colors.ForestGreen, Colors.LimeGreen);
            Assets["road"] = CreateTile(32, 32, Colors.DarkGray, Colors.Gray);
            Assets["water"] = CreateTile(32, 32, Colors.DodgerBlue, Colors.DeepSkyBlue);
            Assets["rail"] = CreateRail();
            Assets["chicken"] = CreateChicken();
            Assets["car"] = CreateCar(Colors.Red);
            Assets["log"] = CreateLog();
            Assets["train"] = CreateTrain();
        }

        private static Bitmap CreateTile(int w, int h, Color baseColor, Color noiseColor)
        {
            var bmp = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
            using (var buf = bmp.Lock())
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        bool noise = (x * 3 + y * 7) % 11 < 2; // Pseudo-random noise
                        SetPixel(buf, x, y, w, noise ? noiseColor : baseColor);
                    }
                }
            }
            return bmp;
        }

        private static Bitmap CreateRail()
        {
            int w = 32, h = 32;
            var bmp = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
            using (var buf = bmp.Lock())
            {
                Color ground = Colors.Tan;
                Color wood = Colors.SaddleBrown;
                Color steel = Colors.Silver;

                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        SetPixel(buf, x, y, w, ground);
                    }
                }
                
                // Sleepers
                FillRect(buf, w, 4, 0, 4, 32, wood);
                FillRect(buf, w, 24, 0, 4, 32, wood);

                // Rails
                FillRect(buf, w, 0, 8, 32, 2, steel);
                FillRect(buf, w, 0, 22, 32, 2, steel);
            }
            return bmp;
        }

        private static Bitmap CreateChicken()
        {
            int w = 32, h = 32;
            var bmp = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
            using (var buf = bmp.Lock())
            {
                // Body
                FillRect(buf, w, 8, 8, 16, 16, Colors.White);
                // Comb
                FillRect(buf, w, 12, 4, 8, 4, Colors.Red);
                // Beak
                FillRect(buf, w, 24, 12, 4, 4, Colors.Orange);
                // Eye
                FillRect(buf, w, 20, 10, 2, 2, Colors.Black);
                // Legs
                FillRect(buf, w, 10, 24, 2, 4, Colors.Orange);
                FillRect(buf, w, 20, 24, 2, 4, Colors.Orange);
            }
            return bmp;
        }

        private static Bitmap CreateCar(Color color)
        {
            // Car size relative to tile 32. Let's make it bigger or fit.
            // Game logic uses 1.5 width (approx 48px if cell is 32, but cell is 40 in main).
            // Let's make a standard texture and scale it or just make it hi-res.
            int w = 60, h = 32;
            var bmp = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
            using (var buf = bmp.Lock())
            {
                FillRect(buf, w, 4, 8, 52, 16, color);
                // Windows
                FillRect(buf, w, 10, 10, 40, 12, Colors.LightBlue);
                // Wheels
                FillRect(buf, w, 8, 22, 8, 8, Colors.Black);
                FillRect(buf, w, 44, 22, 8, 8, Colors.Black);
            }
            return bmp;
        }
        
        private static Bitmap CreateLog()
        {
            int w = 100, h = 32;
            var bmp = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
            using (var buf = bmp.Lock())
            {
                FillRect(buf, w, 0, 4, 100, 24, Colors.SaddleBrown);
                // Texture
                FillRect(buf, w, 10, 4, 2, 24, Colors.Brown);
                FillRect(buf, w, 50, 4, 2, 24, Colors.Brown);
                FillRect(buf, w, 90, 4, 2, 24, Colors.Brown);
            }
            return bmp;
        }
        
        private static Bitmap CreateTrain()
        {
            int w = 128, h = 32; // Texture repeats or stretch?
            // Actually train is just a long rect in game.
            var bmp = new WriteableBitmap(new PixelSize(w, h), new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);
            using (var buf = bmp.Lock())
            {
                FillRect(buf, w, 0, 2, 128, 28, Colors.DarkSlateGray);
                FillRect(buf, w, 0, 10, 128, 4, Colors.Red);
                // Windows
                for(int i=10; i<120; i+=30)
                    FillRect(buf, w, i, 5, 20, 10, Colors.LightBlue);
            }
            return bmp;
        }

        private unsafe static void SetPixel(ILockedFramebuffer buf, int x, int y, int strideWidth, Color color)
        {
            if (x < 0 || y < 0 || x >= strideWidth) return;
            var ptr = (uint*)buf.Address;
            // R G B A
            // Avalonia Rgba8888
            uint pixel = (uint)((color.A << 24) | (color.B << 16) | (color.G << 8) | color.R);
            ptr[y * strideWidth + x] = pixel;
        }

        private static void FillRect(ILockedFramebuffer buf, int strideWidth, int x, int y, int w, int h, Color color)
        {
            for(int i=0; i<w; i++)
            {
                for(int j=0; j<h; j++)
                {
                    SetPixel(buf, x+i, y+j, strideWidth, color);
                }
            }
        }
    }
}