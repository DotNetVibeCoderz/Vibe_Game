using System;
using System.Media;
using System.IO;

namespace SpaceShooter
{
    public static class SoundManager
    {
        // To use sounds, place shoot.wav, explosion.wav, and boss.wav in the execution folder.
        public static void PlayShoot() { Play("shoot.wav"); }
        public static void PlayExplosion() { Play("explosion.wav"); }
        public static void PlayBoss() { Play("boss.wav"); }

        private static void Play(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    using (SoundPlayer sp = new SoundPlayer(fileName))
                    {
                        sp.Play();
                    }
                }
            }
            catch { /* Ignore if sound fails to play */ }
        }
    }
}
