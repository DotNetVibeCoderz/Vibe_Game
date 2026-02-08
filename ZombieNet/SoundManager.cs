using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace ZombieNet
{
    public static class SoundManager
    {
        private static SoundEffect _shootSound;
        private static SoundEffect _hitSound;

        public static void LoadContent(ContentManager content)
        {
            // Check if sound files exist, if not generate them
            string shootPath = "Content/shoot.wav";
            string hitPath = "Content/hit.wav";

            if (!File.Exists(shootPath))
            {
                // Ensure directory exists
                Directory.CreateDirectory("Content");
                SoundGenerator.GenerateShootSound(shootPath);
            }

            if (!File.Exists(hitPath))
            {
                SoundGenerator.GenerateHitSound(hitPath);
            }

            // Load sounds from raw wav files
            // Note: Content.Load<SoundEffect> usually requires .xnb
            // We can use SoundEffect.FromStream for raw wavs
            using (var stream = new FileStream(shootPath, FileMode.Open))
            {
                _shootSound = SoundEffect.FromStream(stream);
            }

            using (var stream = new FileStream(hitPath, FileMode.Open))
            {
                _hitSound = SoundEffect.FromStream(stream);
            }
        }

        public static void PlayShoot()
        {
            _shootSound?.Play(0.3f, 0.0f, 0.0f);
        }

        public static void PlayHit()
        {
            _hitSound?.Play(0.2f, 0.0f, 0.0f);
        }
    }
}
