using SkiaSharp;

namespace Breakout.GameObjects;

/// <summary>
/// Bola utama dalam game. Memiliki posisi, kecepatan, dan efek trail.
/// </summary>
public class Ball
{
    public SKPoint Position { get; set; }
    public SKPoint Velocity { get; set; }
    public float Radius { get; private set; } = 8;
    public float Speed { get; private set; }
    public bool IsLost { get; set; }
    public bool IsStuck { get; private set; }

    private readonly float _baseSpeed;
    private readonly Random _random = new();
    private float _rotation;

    // Warna-warna neon untuk efek
    private static readonly SKColor[] NeonColors = new[]
    {
        new SKColor(255, 0, 128),   // Pink
        new SKColor(0, 255, 255),   // Cyan
        new SKColor(255, 255, 0),   // Yellow
        new SKColor(0, 255, 128),   // Green
        new SKColor(255, 128, 0),   // Orange
    };

    private SKColor _currentColor;
    private float _colorTimer;

    public Ball(float x, float y, float speedMultiplier = 1.0f)
    {
        Position = new SKPoint(x, y);
        _baseSpeed = 350 * speedMultiplier; // 350 pixels/sec base speed
        Speed = _baseSpeed;
        IsStuck = true;
        _currentColor = NeonColors[0];
    }

    /// <summary>
    /// Reset bola ke posisi awal (menempel di paddle).
    /// </summary>
    public void Reset(float paddleX, float paddleY)
    {
        Position = new SKPoint(paddleX, paddleY - 20);
        Velocity = SKPoint.Empty;
        IsStuck = true;
        IsLost = false;
        Speed = _baseSpeed;
    }

    /// <summary>
    /// Luncurkan bola dari paddle.
    /// </summary>
    public void Launch()
    {
        if (!IsStuck) return;

        // Arah acak antara -30° sampai 30° dari vertikal
        float angle = -MathF.PI / 2 + (float)(_random.NextDouble() - 0.5) * MathF.PI / 3;
        Velocity = new SKPoint(
            MathF.Cos(angle) * Speed,
            MathF.Sin(angle) * Speed
        );
        IsStuck = false;
    }

    /// <summary>
    /// Update posisi bola setiap frame.
    /// </summary>
    public void Update(float dt, float gameWidth, float gameHeight)
    {
        if (IsStuck || IsLost) return;

        // Update posisi
        Position = new SKPoint(
            Position.X + Velocity.X * dt,
            Position.Y + Velocity.Y * dt
        );

        // Rotasi untuk efek visual
        _rotation += dt * Speed * 0.01f;

        // Ganti warna perlahan
        _colorTimer += dt;
        if (_colorTimer > 0.5f)
        {
            _colorTimer = 0;
            _currentColor = NeonColors[_random.Next(NeonColors.Length)];
        }

        // Pantulan di dinding kiri/kanan
        if (Position.X - Radius <= 0)
        {
            Position = new SKPoint(Radius, Position.Y);
            Velocity = new SKPoint(-Velocity.X, Velocity.Y);
        }
        else if (Position.X + Radius >= gameWidth)
        {
            Position = new SKPoint(gameWidth - Radius, Position.Y);
            Velocity = new SKPoint(-Velocity.X, Velocity.Y);
        }

        // Pantulan di dinding atas
        if (Position.Y - Radius <= 0)
        {
            Position = new SKPoint(Position.X, Radius);
            Velocity = new SKPoint(Velocity.X, -Velocity.Y);
        }

        // Bola jatuh ke bawah (lost)
        if (Position.Y - Radius > gameHeight)
        {
            IsLost = true;
        }

        // Pastikan bola tidak bergerak horizontal lurus
        if (MathF.Abs(Velocity.X) < 30)
        {
            Velocity = new SKPoint(
                Velocity.X + (Velocity.X >= 0 ? 30 : -30),
                Velocity.Y
            );
            // Normalize
            float len = MathF.Sqrt(Velocity.X * Velocity.X + Velocity.Y * Velocity.Y);
            Velocity = new SKPoint(Velocity.X / len * Speed, Velocity.Y / len * Speed);
        }
    }

    /// <summary>
    /// Render bola dengan efek glow.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        // Glow effect
        using (var glowPaint = new SKPaint
        {
            Color = _currentColor.WithAlpha(60),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12)
        })
        {
            canvas.DrawCircle(Position, Radius + 6, glowPaint);
        }

        // Outer glow
        using (var outerPaint = new SKPaint
        {
            Color = _currentColor.WithAlpha(120),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6)
        })
        {
            canvas.DrawCircle(Position, Radius + 3, outerPaint);
        }

        // Bola utama
        using (var bodyPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true
        })
        {
            canvas.DrawCircle(Position, Radius, bodyPaint);
        }

        // Highlight
        using (var highlightPaint = new SKPaint
        {
            Color = _currentColor,
            IsAntialias = true
        })
        {
            canvas.DrawCircle(new SKPoint(Position.X - 2, Position.Y - 2), Radius * 0.4f, highlightPaint);
        }
    }
}
