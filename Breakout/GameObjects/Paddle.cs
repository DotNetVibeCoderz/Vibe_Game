using SkiaSharp;

namespace Breakout.GameObjects;

/// <summary>
/// Paddle pemain. Bisa digerakkan kiri/kanan dan memiliki efek glow.
/// </summary>
public class Paddle
{
    public SKPoint Position { get; private set; }
    public SKRect Bounds => new(Position.X - Width / 2, Position.Y - Height / 2,
                                 Position.X + Width / 2, Position.Y + Height / 2);

    public float Width { get; private set; } = 100;
    public float Height { get; private set; } = 16;
    private float _baseWidth = 100;

    private float _moveSpeed = 500;
    private float _glowIntensity = 0.5f;
    private float _glowDirection = 1;

    // Warna neon untuk paddle
    private static readonly SKColor PaddleColor = new(0, 200, 255);
    private static readonly SKColor PaddleGlowColor = new(0, 200, 255, 80);

    public Paddle(float x, float y)
    {
        Position = new SKPoint(x, y);
    }

    public void Reset(float x, float y)
    {
        Position = new SKPoint(x, y);
        ResetSize();
    }

    public void ResetSize()
    {
        Width = _baseWidth;
    }

    public void SetSizeMultiplier(float multiplier)
    {
        Width = _baseWidth * multiplier;
    }

    /// <summary>
    /// Update posisi paddle berdasarkan input.
    /// </summary>
    public void Update(float dt, float direction, float gameWidth)
    {
        // Gerakan paddle
        float newX = Position.X + direction * _moveSpeed * dt;
        newX = Math.Clamp(newX, Width / 2, gameWidth - Width / 2);
        Position = new SKPoint(newX, Position.Y);

        // Animasi glow berdenyut
        _glowIntensity += _glowDirection * dt * 2;
        if (_glowIntensity > 1.0f) { _glowIntensity = 1.0f; _glowDirection = -1; }
        if (_glowIntensity < 0.3f) { _glowIntensity = 0.3f; _glowDirection = 1; }
    }

    /// <summary>
    /// Render paddle dengan efek glow neon.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        var bounds = Bounds;
        float cornerRadius = 6;

        // Glow luar
        using (var glowPaint = new SKPaint
        {
            Color = PaddleGlowColor.WithAlpha((byte)(80 * _glowIntensity)),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15 * _glowIntensity)
        })
        {
            canvas.DrawRoundRect(bounds, cornerRadius + 4, cornerRadius + 4, glowPaint);
        }

        // Glow sedang
        using (var midGlowPaint = new SKPaint
        {
            Color = new SKColor(0, 220, 255, (byte)(120 * _glowIntensity)),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
        })
        {
            canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, midGlowPaint);
        }

        // Paddle utama (gradient)
        using (var bodyPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(bounds.Left, bounds.Top),
                new SKPoint(bounds.Left, bounds.Bottom),
                new[] { new SKColor(0, 240, 255), new SKColor(0, 150, 200) },
                SKShaderTileMode.Clamp)
        })
        {
            canvas.DrawRoundRect(bounds, cornerRadius, cornerRadius, bodyPaint);
        }

        // Garis highlight di atas
        using (var highlightPaint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 100),
            IsAntialias = true
        })
        {
            var highlightRect = new SKRect(bounds.Left + 10, bounds.Top + 2,
                                           bounds.Right - 10, bounds.Top + 5);
            canvas.DrawRoundRect(highlightRect, 2, 2, highlightPaint);
        }

        // Ujung-ujung neon
        using (var edgePaint = new SKPaint
        {
            Color = new SKColor(255, 0, 128, 150),
            IsAntialias = true
        })
        {
            canvas.DrawCircle(bounds.Left + 4, bounds.MidY, 3, edgePaint);
            canvas.DrawCircle(bounds.Right - 4, bounds.MidY, 3, edgePaint);
        }
    }
}
