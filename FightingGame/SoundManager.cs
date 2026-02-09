using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace FightingGame
{
    public enum SoundType
    {
        Jump,
        Punch,
        Kick,
        Hit,
        Block,
        Win,
        Lose
    }

    public static class SoundGenerator
    {
        private const int SampleRate = 44100;
        private const short BitsPerSample = 16;

        public static Stream GenerateSound(SoundType type)
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            // Placeholder for header
            writer.Write(new byte[44]);

            int samples = 0;

            switch (type)
            {
                case SoundType.Jump:
                    samples = GenerateSlide(writer, 400, 800, 0.2f);
                    break;
                case SoundType.Punch:
                    samples = GenerateSquareWave(writer, 150, 0.1f, 0.5f);
                    break;
                case SoundType.Kick:
                    samples = GenerateSquareWave(writer, 100, 0.15f, 0.6f);
                    break;
                case SoundType.Hit:
                    samples = GenerateNoise(writer, 0.2f);
                    break;
                case SoundType.Block:
                    samples = GenerateSquareWave(writer, 50, 0.05f, 0.8f); // Short thud
                    break;
                case SoundType.Win:
                    samples = GenerateArpeggio(writer, new[] { 523, 659, 784, 1046 }, 0.15f); // C Major
                    break;
                case SoundType.Lose:
                    samples = GenerateArpeggio(writer, new[] { 392, 370, 349, 330 }, 0.2f); // Descending
                    break;
            }

            // Fill header
            stream.Seek(0, SeekOrigin.Begin);
            WriteWavHeader(writer, samples);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        private static int GenerateSquareWave(BinaryWriter writer, double frequency, float duration, float volume = 0.5f)
        {
            int sampleCount = (int)(SampleRate * duration);
            for (int i = 0; i < sampleCount; i++)
            {
                double t = (double)i / SampleRate;
                short sample = (short)(volume * (Math.Sin(2 * Math.PI * frequency * t) > 0 ? 10000 : -10000));
                
                // Simple decay
                sample = (short)(sample * (1.0 - (double)i / sampleCount));
                
                writer.Write(sample);
            }
            return sampleCount;
        }

        private static int GenerateSlide(BinaryWriter writer, double startFreq, double endFreq, float duration)
        {
            int sampleCount = (int)(SampleRate * duration);
            for (int i = 0; i < sampleCount; i++)
            {
                double progress = (double)i / sampleCount;
                double frequency = startFreq + (endFreq - startFreq) * progress;
                double t = (double)i / SampleRate;
                
                short sample = (short)(0.3f * (Math.Sin(2 * Math.PI * frequency * t) > 0 ? 10000 : -10000));
                writer.Write(sample);
            }
            return sampleCount;
        }

        private static int GenerateNoise(BinaryWriter writer, float duration)
        {
            int sampleCount = (int)(SampleRate * duration);
            var rng = new Random();
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = (short)(rng.Next(-5000, 5000));
                // Decay
                sample = (short)(sample * (1.0 - (double)i / sampleCount));
                writer.Write(sample);
            }
            return sampleCount;
        }

        private static int GenerateArpeggio(BinaryWriter writer, int[] freqs, float noteDuration)
        {
            int totalSamples = 0;
            foreach (var freq in freqs)
            {
                totalSamples += GenerateSquareWave(writer, freq, noteDuration, 0.4f);
            }
            return totalSamples;
        }

        private static void WriteWavHeader(BinaryWriter writer, int sampleCount)
        {
            int byteRate = SampleRate * BitsPerSample / 8;
            int blockAlign = BitsPerSample / 8;
            int subChunk2Size = sampleCount * blockAlign;
            int chunkSize = 36 + subChunk2Size;

            writer.Write("RIFF".ToCharArray());
            writer.Write(chunkSize);
            writer.Write("WAVE".ToCharArray());
            writer.Write("fmt ".ToCharArray());
            writer.Write(16); // SubChunk1Size
            writer.Write((short)1); // AudioFormat (PCM)
            writer.Write((short)1); // NumChannels (Mono)
            writer.Write(SampleRate);
            writer.Write(byteRate);
            writer.Write((short)blockAlign);
            writer.Write(BitsPerSample);
            writer.Write("data".ToCharArray());
            writer.Write(subChunk2Size);
        }
    }

    public class SoundManager
    {
        public void Play(SoundType type)
        {
            Task.Run(() =>
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {
                        using (var stream = SoundGenerator.GenerateSound(type))
                        using (var player = new SoundPlayer(stream))
                        {
                            player.Play();
                        }
                    }
                }
                catch
                {
                    // Ignore errors if sound fails (e.g. Linux/Mac without libraries)
                }
            });
        }
    }
}