using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Maze
{
    // Class ini biar kita bisa gambar pixel super cepat,
    // kalau pakai SetPixel biasa, game-nya bakal jadi slide show :D
    public class DirectBitmap : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Bits { get; private set; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public DirectBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }

        public void SetPixel(int x, int y, int colour)
        {
            int index = x + (y * Width);
            if (index >= 0 && index < Bits.Length)
            {
                Bits[index] = colour;
            }
        }

        public int GetPixel(int x, int y)
        {
            int index = x + (y * Width);
            if (index >= 0 && index < Bits.Length)
            {
                return Bits[index];
            }
            return 0;
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            BitsHandle.Free();
        }
    }
}