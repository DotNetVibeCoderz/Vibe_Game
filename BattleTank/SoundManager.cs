using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace BattleTank
{
    public static class SoundManager
    {
        public static SoundEffect Shoot;
        public static SoundEffect Explosion;
        public static SoundEffect GameOver;

        public static void Initialize(Microsoft.Xna.Framework.Game game)
        {
            // We generate WAV data in memory and load it as SoundEffect
            Shoot = CreateSound(SoundType.Shoot);
            Explosion = CreateSound(SoundType.Explosion);
            GameOver = CreateSound(SoundType.GameOver);
        }

        private enum SoundType { Shoot, Explosion, GameOver }

        private static SoundEffect CreateSound(SoundType type)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);
                int sampleRate = 44100;
                short bitsPerSample = 16;
                short channels = 1;

                // Write WAV Header placeholder
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(0); // ChunkSize
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Subchunk1Size
                writer.Write((short)1); // AudioFormat (1 = PCM)
                writer.Write(channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * bitsPerSample / 8); // ByteRate
                writer.Write((short)(channels * bitsPerSample / 8)); // BlockAlign
                writer.Write(bitsPerSample);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
                writer.Write(0); // Subchunk2Size

                int dataStartPosition = (int)stream.Position;

                // Generate Data
                switch (type)
                {
                    case SoundType.Shoot:
                        GenerateShoot(writer, sampleRate);
                        break;
                    case SoundType.Explosion:
                        GenerateExplosion(writer, sampleRate);
                        break;
                    case SoundType.GameOver:
                        GenerateGameOver(writer, sampleRate);
                        break;
                }

                int dataEndPosition = (int)stream.Position;
                int dataSize = dataEndPosition - dataStartPosition;

                // Update Header
                writer.Seek(4, SeekOrigin.Begin);
                writer.Write(36 + dataSize);
                writer.Seek(40, SeekOrigin.Begin);
                writer.Write(dataSize);

                stream.Position = 0;
                return SoundEffect.FromStream(stream);
            }
        }

        private static void GenerateShoot(BinaryWriter writer, int sampleRate)
        {
            // Pew sound: High freq sliding down quickly
            int duration = sampleRate / 8; // 0.125s
            for (int i = 0; i < duration; i++)
            {
                double t = (double)i / duration;
                double freq = 800 - (t * 600); // 800Hz to 200Hz
                short sample = (short)(Math.Sign(Math.Sin(2 * Math.PI * freq * (double)i / sampleRate)) * 10000);
                // Decay
                sample = (short)(sample * (1.0 - t));
                writer.Write(sample);
            }
        }

        private static void GenerateExplosion(BinaryWriter writer, int sampleRate)
        {
            // Noise decay
            int duration = sampleRate / 2; // 0.5s
            Random rnd = new Random();
            for (int i = 0; i < duration; i++)
            {
                double t = (double)i / duration;
                short sample = (short)rnd.Next(-10000, 10000);
                // Decay
                sample = (short)(sample * (1.0 - t));
                writer.Write(sample);
            }
        }

        private static void GenerateGameOver(BinaryWriter writer, int sampleRate)
        {
            // Sad slide
            int duration = sampleRate; // 1s
            for (int i = 0; i < duration; i++)
            {
                double t = (double)i / duration;
                double freq = 300 - (t * 200);
                short sample = (short)(Math.Sign(Math.Sin(2 * Math.PI * freq * (double)i / sampleRate)) * 10000);
                writer.Write(sample);
            }
        }
    }
}