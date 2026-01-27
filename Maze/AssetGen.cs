using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;

namespace Maze
{
    public static class AssetGen
    {
        // Generate texture dengan warna custom
        public static DirectBitmap GenerateWallTexture(int size, Color baseColor, Color mortarColor)
        {
            DirectBitmap texture = new DirectBitmap(size, size);
            Random rnd = new Random();
            
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // Pola bata
                    bool isMortar = (y % 16 == 0) || (y % 32 < 16 && x % 32 == 0) || (y % 32 >= 16 && (x + 16) % 32 == 0);
                    
                    int color;
                    if (isMortar)
                    {
                        color = mortarColor.ToArgb();
                    }
                    else
                    {
                        // Noise untuk tekstur bata biar tidak flat
                        int noise = rnd.Next(-10, 10);
                        int r = Math.Min(255, Math.Max(0, baseColor.R + noise));
                        int g = Math.Min(255, Math.Max(0, baseColor.G + noise));
                        int b = Math.Min(255, Math.Max(0, baseColor.B + noise));
                        color = Color.FromArgb(r, g, b).ToArgb();
                    }
                    texture.SetPixel(x, y, color);
                }
            }
            return texture;
        }

        public static DirectBitmap GeneratePixelArtTexture(int size, string type)
        {
            DirectBitmap texture = new DirectBitmap(size, size);
            
            int colorMain = 0;
            if (type == "Treasure") colorMain = Color.Gold.ToArgb();
            if (type == "Finish") colorMain = Color.Lime.ToArgb();
            if (type == "Door") colorMain = Color.SaddleBrown.ToArgb();

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    // Default transparent black
                    texture.SetPixel(x, y, 0); 
                    
                    if (type == "Treasure")
                    {
                        // Gambar Piala sederhana
                        int dx = x - size / 2;
                        int dy = y - size / 2;
                        // Cup body
                        if (Math.Abs(dx) < size / 4 && dy > 0 && dy < size / 3) texture.SetPixel(x, y, colorMain); 
                        // Cup top
                        if (dx * dx + dy * dy < (size / 3) * (size / 3) && dy <= 0) texture.SetPixel(x, y, colorMain);
                        // Cup base
                        if (dy > size/3 && dy < size/2 && Math.Abs(dx) < size/8) texture.SetPixel(x, y, colorMain);
                    }
                    else if (type == "Door")
                    {
                        // Pintu Exit Style
                        if (x > 2 && x < size - 2 && y > 2 && y < size - 2)
                        {
                            bool border = x < 5 || x > size - 5 || y < 5 || y > size - 5;
                            texture.SetPixel(x, y, border ? Color.Silver.ToArgb() : Color.FromArgb(60, 60, 70).ToArgb());
                            
                            // Gagang pintu
                            if (x > size - 10 && x < size - 6 && y == size / 2) texture.SetPixel(x, y, Color.Yellow.ToArgb());
                            
                            // Exit text "fake" (stripes)
                            if (x > size/4 && x < size*3/4 && y > size/5 && y < size/3 && (x%4!=0)) texture.SetPixel(x, y, Color.Lime.ToArgb()); 
                        }
                    }
                }
            }
            return texture;
        }

        // Method untuk generate file WAV sederhana biar ada suaranya (RE-ADDED)
        public static void CreateSoundFile(string filePath, int frequency, int durationMs)
        {
            int sampleRate = 44100;
            short bitsPerSample = 16;
            int subChunk2Size = sampleRate * durationMs / 1000 * bitsPerSample / 8;
            int chunkSize = 36 + subChunk2Size;

            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(chunkSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1); // PCM
                writer.Write((short)1); // Mono
                writer.Write(sampleRate);
                writer.Write(sampleRate * bitsPerSample / 8);
                writer.Write((short)(bitsPerSample / 8));
                writer.Write(bitsPerSample);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(subChunk2Size);

                double theta = frequency * 2 * Math.PI / sampleRate;
                for (int i = 0; i < subChunk2Size / 2; i++)
                {
                    short amplitude = (short)(short.MaxValue * Math.Sin(theta * i));
                    writer.Write(amplitude);
                }
            }
        }
    }
}