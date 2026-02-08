using Microsoft.Xna.Framework.Audio;
using System;
using System.IO;

namespace ParticleCombat
{
    public static class SoundSystem
    {
        public static SoundEffect Shoot { get; private set; }
        public static SoundEffect Explosion { get; private set; }
        public static SoundEffect Spawn { get; private set; }
        public static SoundEffect PowerUp { get; private set; }

        public static void Load()
        {
            try 
            {
                Shoot = GenerateSound(SoundType.Shoot);
                Explosion = GenerateSound(SoundType.Explosion);
                Spawn = GenerateSound(SoundType.Spawn);
                PowerUp = GenerateSound(SoundType.PowerUp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to generate sounds: " + ex.Message);
            }
        }

        public static void Play(SoundEffect effect, float volume = 0.3f, float pitch = 0.0f, float pan = 0.0f)
        {
            if (effect != null)
            {
                try
                {
                    effect.Play(volume, pitch, pan);
                }
                catch { /* Ignore audio errors */ }
            }
        }

        private enum SoundType { Shoot, Explosion, Spawn, PowerUp }

        private static SoundEffect GenerateSound(SoundType type)
        {
            // Format: PCM, 1 channel, 44100Hz, 16-bit
            int sampleRate = 44100;
            // Shoot: Short, Explosion: Medium, Spawn: Long
            int durationMs = type == SoundType.Explosion ? 400 : (type == SoundType.Spawn ? 500 : (type == SoundType.PowerUp ? 600 : 100));
            int sampleCount = (sampleRate * durationMs) / 1000;
            
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                // RIFF Header
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + sampleCount * 2); // File size
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16); // PCM chunk size
                writer.Write((short)1); // PCM format
                writer.Write((short)1); // Channels (Mono)
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2); // Byte rate
                writer.Write((short)2); // Block align
                writer.Write((short)16); // Bits per sample
                writer.Write("data".ToCharArray());
                writer.Write(sampleCount * 2);

                Random rand = new Random();

                for (int i = 0; i < sampleCount; i++)
                {
                    double t = (double)i / sampleRate;
                    double totalDuration = durationMs / 1000.0;
                    short sample = 0;

                    if (type == SoundType.Shoot)
                    {
                        // Frequency sweep down (Laser pew) - Square wave
                        double freq = 1200.0 - (1000.0 * (t / totalDuration));
                        sample = (short)(Math.Sin(t * freq * 2.0 * Math.PI) > 0 ? 6000 : -6000);
                        // Decay volume
                        sample = (short)(sample * (1.0 - t / totalDuration));
                    }
                    else if (type == SoundType.Explosion)
                    {
                        // White noise
                        double noise = rand.NextDouble() * 2.0 - 1.0;
                        // Exponential decay
                        double envelope = Math.Exp(-8.0 * t);
                        sample = (short)(noise * 12000 * envelope);
                    }
                    else if (type == SoundType.Spawn)
                    {
                         // Frequency sweep up
                         double freq = 200.0 + (400.0 * (t / totalDuration));
                         // Sine wave
                         sample = (short)(Math.Sin(t * freq * 2.0 * Math.PI) * 8000);
                         // Triangle envelope
                         if (t < totalDuration / 2)
                             sample = (short)(sample * (t / (totalDuration/2)));
                         else
                             sample = (short)(sample * (1.0 - ((t - totalDuration/2) / (totalDuration/2))));
                    }
                    else if (type == SoundType.PowerUp)
                    {
                        // Arpeggio / Coin sound
                        double freq = 440.0;
                        if (t > totalDuration * 0.5) freq = 880.0;
                        sample = (short)(Math.Sin(t * freq * 2.0 * Math.PI) * 8000);
                        sample = (short)(sample * (1.0 - t / totalDuration));
                    }

                    writer.Write(sample);
                }
                
                writer.Flush();
                stream.Position = 0;
                return SoundEffect.FromStream(stream);
            }
        }
    }
}