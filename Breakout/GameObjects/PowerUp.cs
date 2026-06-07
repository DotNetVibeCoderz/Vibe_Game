using SkiaSharp;

namespace Breakout.GameObjects;

/// <summary>
/// Item power-up yang jatuh dari brick yang dihancurkan.
/// Pemain bisa menangkapnya dengan paddle.
/// </summary>
public enum PowerUpType
{
    PaddleIncrease,
    PaddleDecrease,
    MultiBall,
    Laser,
    SlowMotion
}

public class PowerUp
{
    public SKPoint Position { get; private set; }
    public SKRect Bounds => new(Position.X - 10, Position.Y - 10,
                                 Position.X + 10, Position.Y + 10);
    public PowerUpType Type { get; private set; }
    public bool IsCollected { get; private set; }

    private float _speed = 120; // pixels/sec
    private float _rotation;
    private float _pulseTimer;
    private readonly Random _random = new();

    // Warna dan ikon berdasarkan tipe
    private static readonly Dictionary<PowerUpType, (SKColor Color, string Icon)> PowerUpData = new()
    {
        [PowerUpType.PaddleIncrease] = (new SKColor(0, 255, 200), "⬌"),
        [PowerUpType.PaddleDecrease] = (new SKColor(255, 100, 0), "⬍"),
        [PowerUpType.MultiBall] = (new SKColor(255, 255, 0), "⚬"),
        [PowerUpType.Laser] = (new SKColor(255, 0, 128), "⚡"),
        [PowerUpType.SlowMotion] = (new SKColor(0, 150, 255), "◷"),
    };

    public PowerUp(float x, float y, PowerUpType type)
    {
        Position = new SKPoint(x, y);
        Type = type;
    }

    /// <summary>
    /// Update posisi power-up (jatuh ke bawah).
    /// </summary>
    public void Update(float dt)
    {
        Position = new SKPoint(Position.X, Position.Y + _speed * dt);
        _rotation += dt * 3;
        _pulseTimer += dt;
    }

    /// <summary>
    /// Cek apakah power-up sudah keluar layar.
    /// </summary>
    public bool IsOffScreen(float gameHeight) => Position.Y > gameHeight + 20;

    /// <summary>
    /// Tandai power-up sudah diambil.
    /// </summary>
    public void Collect() => IsCollected = true;

    /// <summary>
    /// Render power-up.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        if (IsCollected) return;

        var (color, icon) = PowerUpData[Type];
        float pulse = MathF.Sin(_pulseTimer * 4) * 2;

        // Glow
        using (var glowPaint = new SKPaint
        {
            Color = color.WithAlpha(60),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10)
        })
        {
            canvas.DrawCircle(Position, 14 + pulse, glowPaint);
        }

        // Body
        using (var bodyPaint = new SKPaint
        {
            Color = color.WithAlpha(200),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3)
        })
        {
            canvas.DrawCircle(Position, 10 + pulse, bodyPaint);
        }

        // Lingkaran dalam
        using (var innerPaint = new SKPaint
        {
            Color = SKColors.White.WithAlpha(180),
            IsAntialias = true
        })
        {
            canvas.DrawCircle(Position, 7 + pulse * 0.5f, innerPaint);
        }

        // Ikon
        using (var iconPaint = new SKPaint
        {
            Color = color,
            IsAntialias = true,
            TextSize = 14,
            TextAlign = SKTextAlign.Center
        })
        {
            canvas.DrawText(icon, Position.X, Position.Y + 5, iconPaint);
        }
    }
}
