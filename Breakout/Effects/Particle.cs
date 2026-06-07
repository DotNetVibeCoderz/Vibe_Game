using SkiaSharp;

namespace Breakout.Effects;

/// <summary>
/// Partikel tunggal untuk sistem partikel.
/// </summary>
public class Particle
{
    public SKPoint Position { get; set; }
    public SKPoint Velocity { get; set; }
    public float Life { get; set; }
    public float MaxLife { get; set; }
    public float Size { get; set; }
    public SKColor Color { get; set; }
    public SKColor StartColor { get; set; }
    public bool IsAlive => Life > 0;

    public Particle(SKPoint position, SKPoint velocity, float life, float size, SKColor color)
    {
        Position = position;
        Velocity = velocity;
        Life = life;
        MaxLife = life;
        Size = size;
        Color = color;
        StartColor = color;
    }

    /// <summary>
    /// Update partikel. Mengembalikan false jika partikel mati.
    /// </summary>
    public bool Update(float dt)
    {
        Life -= dt;
        if (Life <= 0) return false;

        Position = new SKPoint(
            Position.X + Velocity.X * dt,
            Position.Y + Velocity.Y * dt
        );

        // Perlambat
        Velocity = new SKPoint(
            Velocity.X * 0.98f,
            Velocity.Y * 0.98f
        );

        // Fade out
        float alpha = Life / MaxLife;
        Color = StartColor.WithAlpha((byte)(StartColor.Alpha * alpha));

        return true;
    }

    /// <summary>
    /// Render partikel.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        using var paint = new SKPaint
        {
            Color = Color,
            IsAntialias = true
        };
        canvas.DrawCircle(Position, Size * (Life / MaxLife), paint);
    }
}
