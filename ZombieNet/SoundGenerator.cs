using System;
using System.IO;
using System.Text;

namespace ZombieNet
{
    public static class SoundGenerator
    {
        public static void GenerateShootSound(string filepath)
        {
            // Simple decay wave (pew)
            int sampleRate = 44100;
            short bitsPerSample = 16;
            short channels = 1;
            int durationMs = 200;
            int numSamples = sampleRate * durationMs / 1000;
            
            using (var stream = new FileStream(filepath, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                WriteWavHeader(writer, numSamples, sampleRate, channels, bitsPerSample);
                
                for (int i = 0; i < numSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    double freq = 800 - (t * 3000); // Pitch drop
                    if (freq < 100) freq = 100;
                    double volume = 1.0 - (double)i / numSamples;
                    short sample = (short)(Math.Sin(2 * Math.PI * freq * t) * 32000 * volume);
                    writer.Write(sample);
                }
            }
        }

        public static void GenerateHitSound(string filepath)
        {
            // Noise (thud/hit)
            int sampleRate = 44100;
            short bitsPerSample = 16;
            short channels = 1;
            int durationMs = 100;
            int numSamples = sampleRate * durationMs / 1000;
            Random rnd = new Random();

            using (var stream = new FileStream(filepath, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                WriteWavHeader(writer, numSamples, sampleRate, channels, bitsPerSample);
                
                for (int i = 0; i < numSamples; i++)
                {
                    double volume = 1.0 - (double)i / numSamples;
                    short sample = (short)((rnd.NextDouble() * 2.0 - 1.0) * 20000 * volume);
                    writer.Write(sample);
                }
            }
        }

        private static void WriteWavHeader(BinaryWriter writer, int numSamples, int sampleRate, short channels, short bitsPerSample)
        {
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            short blockAlign = (short)(channels * bitsPerSample / 8);
            int subChunk2Size = numSamples * channels * bitsPerSample / 8;
            int chunkSize = 36 + subChunk2Size;

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(chunkSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16); // Subchunk1Size
            writer.Write((short)1); // AudioFormat (PCM)
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(subChunk2Size);
        }
    }
}
