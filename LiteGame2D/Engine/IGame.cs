using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;

namespace LiteGame2D.Engine
{
    public interface IGame
    {
        void Initialize();
        void Update(double deltaTime);
        void Draw(DrawingContext context, Size screenSize);
        void Reset();
    }

    public static class AssetManager
    {
        public static IImage GetPlaceholder(Color color, int width, int height)
        {
            var pixelSize = new PixelSize(width, height);
            var bitmap = new WriteableBitmap(pixelSize, new Vector(96, 96), PixelFormat.Rgba8888, AlphaFormat.Premul);

            using (var fb = bitmap.Lock())
            {
                // Simple solid color fill
                var data = new uint[width * height];
                uint c = (uint)((color.A << 24) | (color.R << 16) | (color.G << 8) | color.B);
                
                for(int i=0; i<data.Length; i++) data[i] = c;

                System.Runtime.InteropServices.Marshal.Copy((int[])(object)data, 0, fb.Address, data.Length);
            }
            return bitmap;
        }
    }
}