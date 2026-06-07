using SkiaSharp;

namespace Breakout.GameObjects;

/// <summary>
/// Proyektil laser yang ditembakkan dari paddle saat power-up Laser aktif.
/// </summary>
public class Laser
{
    public SKPoint Position { get; private set; }
    public SKRect Bounds => new(Position.X - 3, Position.Y - 8,
                                 Position.X + 3, Position.Y + 8);

    private float _speed = 600; // pixels/sec

    public Laser(float x, float y)
    {
        Position = new SKPoint(x, y);
    }

    /// <summary>
    /// Update posisi laser (bergerak ke atas).
    /// </summary>
    public void Update(float dt)
    {
        Position = new SKPoint(Position.X, Position.Y - _speed * dt);
    }

    /// <summary>
    /// Cek apakah laser sudah keluar layar.
    /// </summary>
    public bool IsOffScreen(float gameHeight) => Position.Y < -20;

    /// <summary>
    /// Render laser dengan efek neon.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        // Glow luar
        using (var glowPaint = new SKPaint
        {
            Color = new SKColor(255, 50, 50, 60),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
        })
        {
            canvas.DrawRoundRect(Bounds, 3, 3, glowPaint);
        }

        // Body laser
        using (var bodyPaint = new SKPaint
        {
            Color = new SKColor(255, 100, 100),
            IsAntialias = true
        })
        {
            canvas.DrawRoundRect(Bounds, 2, 2, bodyPaint);
        }

        // Core putih
        using (var corePaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        })
        {
            var core = new SKRect(Position.X - 1, Position.Y - 6,
                                  Position.X + 1, Position.Y + 6);
            canvas.DrawRoundRect(core, 1, 1, corePaint);
        }
    }
}
