using Breakout.GameObjects;
using SkiaSharp;

namespace Breakout.Effects;

/// <summary>
/// Sistem partikel untuk berbagai efek visual: ledakan, shatter, power-up, dll.
/// </summary>
public class ParticleSystem
{
    private readonly List<Particle> _particles = new();
    private readonly Random _random = new();

    public void Clear() => _particles.Clear();

    public void AddParticle(Particle p) => _particles.Add(p);

    /// <summary>
    /// Update semua partikel.
    /// </summary>
    public void Update(float dt)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            if (!_particles[i].Update(dt))
                _particles.RemoveAt(i);
        }
    }

    /// <summary>
    /// Render semua partikel.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        foreach (var p in _particles)
            p.Render(canvas);
    }

    /// <summary>
    /// Efek partikel saat brick hancur.
    /// </summary>
    public void CreateBrickDestroyEffect(SKPoint position, SKColor brickColor)
    {
        for (int i = 0; i < 20; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = 50 + (float)_random.NextDouble() * 200;
            float life = 0.3f + (float)_random.NextDouble() * 0.5f;
            float size = 2 + (float)_random.NextDouble() * 4;

            _particles.Add(new Particle(
                position,
                new SKPoint(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                life, size, brickColor
            ));
        }
    }

    /// <summary>
    /// Efek partikel saat brick kena hit (tidak hancur).
    /// </summary>
    public void CreateBrickHitEffect(SKPoint position, SKColor brickColor)
    {
        for (int i = 0; i < 5; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = 30 + (float)_random.NextDouble() * 80;
            float life = 0.1f + (float)_random.NextDouble() * 0.2f;

            _particles.Add(new Particle(
                position,
                new SKPoint(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                life, 2, brickColor
            ));
        }
    }

    /// <summary>
    /// Efek partikel saat bola mengenai paddle.
    /// </summary>
    public void CreatePaddleHitEffect(SKPoint position)
    {
        for (int i = 0; i < 10; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = 30 + (float)_random.NextDouble() * 100;
            float life = 0.2f + (float)_random.NextDouble() * 0.3f;

            _particles.Add(new Particle(
                position,
                new SKPoint(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                life, 2, new SKColor(0, 200, 255, 200)
            ));
        }
    }

    /// <summary>
    /// Efek ledakan untuk explosive brick.
    /// </summary>
    public void CreateExplosionEffect(SKPoint position)
    {
        // Lingkaran ledakan
        for (int i = 0; i < 40; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = 80 + (float)_random.NextDouble() * 300;
            float life = 0.3f + (float)_random.NextDouble() * 0.6f;
            float size = 2 + (float)_random.NextDouble() * 5;

            var color = i % 2 == 0
                ? new SKColor(255, 100, 0, 255)
                : new SKColor(255, 200, 50, 255);

            _particles.Add(new Particle(
                position,
                new SKPoint(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                life, size, color
            ));
        }
    }

    /// <summary>
    /// Efek saat kehilangan nyawa.
    /// </summary>
    public void CreateLifeLostEffect(SKPoint position)
    {
        for (int i = 0; i < 30; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = 50 + (float)_random.NextDouble() * 200;
            float life = 0.5f + (float)_random.NextDouble() * 1.0f;
            float size = 3 + (float)_random.NextDouble() * 5;

            var color = i % 2 == 0
                ? new SKColor(255, 0, 0, 255)
                : new SKColor(255, 100, 100, 255);

            _particles.Add(new Particle(
                position,
                new SKPoint(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                life, size, color
            ));
        }
    }

    /// <summary>
    /// Efek saat level selesai.
    /// </summary>
    public void CreateLevelClearEffect(SKPoint position)
    {
        for (int i = 0; i < 60; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = 50 + (float)_random.NextDouble() * 300;
            float life = 1.0f + (float)_random.NextDouble() * 2.0f;
            float size = 2 + (float)_random.NextDouble() * 6;

            var hue = i * 30; // Rainbow
            var color = new SKColor(
                (byte)(128 + 127 * MathF.Sin(hue * 0.1f)),
                (byte)(128 + 127 * MathF.Sin(hue * 0.1f + 2.094f)),
                (byte)(128 + 127 * MathF.Sin(hue * 0.1f + 4.188f)),
                255
            );

            _particles.Add(new Particle(
                position,
                new SKPoint(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                life, size, color
            ));
        }
    }

    /// <summary>
    /// Efek saat power-up diambil.
    /// </summary>
    public void CreatePowerUpEffect(SKPoint position, PowerUpType type)
    {
        var color = type switch
        {
            PowerUpType.PaddleIncrease => new SKColor(0, 255, 200, 255),
            PowerUpType.PaddleDecrease => new SKColor(255, 100, 0, 255),
            PowerUpType.MultiBall => new SKColor(255, 255, 0, 255),
            PowerUpType.Laser => new SKColor(255, 0, 128, 255),
            PowerUpType.SlowMotion => new SKColor(0, 150, 255, 255),
            _ => new SKColor(255, 255, 255, 255)
        };

        for (int i = 0; i < 25; i++)
        {
            float angle = (float)(_random.NextDouble() * Math.PI * 2);
            float speed = 40 + (float)_random.NextDouble() * 150;
            float life = 0.5f + (float)_random.NextDouble() * 0.8f;

            _particles.Add(new Particle(
                position,
                new SKPoint(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                life, 3, color
            ));
        }
    }
}
