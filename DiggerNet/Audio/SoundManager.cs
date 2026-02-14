using System;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace DiggerNet.Audio
{
    public static class SoundManager
    {
        public static bool IsSoundEnabled { get; set; } = true;

        public static void PlayDig() => PlaySound(400, 50);
        public static void PlayShoot() => PlaySound(800, 100);
        public static void PlayCollect() => PlaySound(1200, 50);
        public static void PlayDie() => PlaySound(200, 500);
        public static void PlayGold() => PlaySound(1500, 150);

        private static void PlaySound(int frequency, int durationMs)
        {
            if (!IsSoundEnabled) return;
            
            // Only Windows supports System.Media.SoundPlayer out of the box
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;

            Task.Run(() => PlayWindowsSound(frequency, durationMs));
        }

        [SupportedOSPlatform("windows")]
        private static void PlayWindowsSound(int frequency, int durationMs)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    WriteWavHeader(ms, frequency, durationMs);
                    ms.Position = 0;
                    using (var player = new SoundPlayer(ms))
                    {
                        player.PlaySync(); 
                    }
                }
            }
            catch { /* Ignore */ }
        }

        private static void WriteWavHeader(Stream stream, int frequency, int durationMs)
        {
            var writer = new BinaryWriter(stream);
            int sampleRate = 44100;
            int numSamples = sampleRate * durationMs / 1000;
            short bitsPerSample = 16;
            short channels = 1;

            writer.Write("RIFF".ToCharArray());
            writer.Write(36 + numSamples * 2);
            writer.Write("WAVEfmt ".ToCharArray());
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * bitsPerSample / 8);
            writer.Write((short)(channels * bitsPerSample / 8));
            writer.Write(bitsPerSample);
            writer.Write("data".ToCharArray());
            writer.Write(numSamples * 2);

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                short sample = (short)(Math.Sin(2 * Math.PI * frequency * t) * 10000); 
                if (sample > 0) sample = 10000;
                else sample = -10000;
                writer.Write(sample);
            }
        }
    }
}