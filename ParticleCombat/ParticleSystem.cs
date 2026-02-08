using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ParticleCombat
{
    public class Particle
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Angle { get; set; }
        public float AngularVelocity { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }
        public int TTL { get; set; } // Time To Live

        public Particle(Texture2D texture, Vector2 position, Vector2 velocity,
            float angle, float angularVelocity, Color color, float size, int ttl)
        {
            Texture = texture;
            Position = position;
            Velocity = velocity;
            Angle = angle;
            AngularVelocity = angularVelocity;
            Color = color;
            Size = size;
            TTL = ttl;
        }

        public void Update()
        {
            TTL--;
            Position += Velocity;
            Angle += AngularVelocity;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle sourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            Vector2 origin = new Vector2(Texture.Width / 2, Texture.Height / 2);

            spriteBatch.Draw(Texture, Position, sourceRectangle, Color,
                Angle, origin, Size, SpriteEffects.None, 0f);
        }
    }

    public static class ParticleManager
    {
        private static List<Particle> particles = new List<Particle>();
        private static Random rand = new Random();

        public static void CreateParticle(Vector2 location, Color color)
        {
            var velocity = new Vector2(
                    1f * (float)(rand.NextDouble() * 2 - 1),
                    1f * (float)(rand.NextDouble() * 2 - 1));

            float angle = 0;
            float angularVelocity = 0.1f * (float)(rand.NextDouble() * 2 - 1);
            float size = (float)rand.NextDouble();
            int ttl = 20 + rand.Next(40);

            // Add new particle
            particles.Add(new Particle(Art.Star, location, velocity, angle, angularVelocity, color, size, ttl));
        }

        public static void Update()
        {
            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update();
                if (particles[particle].TTL <= 0)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }
        }

        public static void Clear()
        {
            particles.Clear();
        }
    }
}