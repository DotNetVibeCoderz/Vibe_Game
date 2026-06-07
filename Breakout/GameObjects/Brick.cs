using Breakout.Core;
using SkiaSharp;

namespace Breakout.GameObjects;

/// <summary>
/// Brick dasar. Bisa dihancurkan oleh bola atau laser.
/// Memiliki berbagai tipe: Standard, MultiHit, Explosive, PowerUp.
/// </summary>
public class Brick
{
    public SKRect Bounds { get; private set; }
    public BrickType BrickType { get; private set; }
    public bool IsDestroyed { get; private set; }
    public int HitPoints { get; private set; }
    public int MaxHitPoints { get; private set; }
    public SKColor Color { get; private set; }
    public int Row { get; set; }
    public int Column { get; set; }

    private float _hitAnimTimer;
    private bool _isHitAnimating;
    private SKColor _originalColor;

    // Palette warna untuk bricks berdasarkan tipe dan baris
    private static readonly SKColor[] StandardColors = new[]
    {
        new SKColor(255, 50, 50),   // Red
        new SKColor(255, 150, 50),  // Orange
        new SKColor(255, 220, 50),  // Yellow
        new SKColor(50, 255, 50),   // Green
        new SKColor(50, 150, 255),  // Blue
        new SKColor(200, 50, 255),  // Purple
        new SKColor(255, 50, 200),  // Pink
    };

    public Brick(float x, float y, float width, float height, BrickType type, int row = 0)
    {
        Bounds = new SKRect(x, y, x + width, y + height);
        BrickType = type;
        Row = row;

        switch (type)
        {
            case BrickType.Standard:
                HitPoints = 1;
                Color = StandardColors[row % StandardColors.Length];
                break;
            case BrickType.MultiHit:
                HitPoints = 3;
                Color = new SKColor(100, 100, 255); // Biru terang
                break;
            case BrickType.Explosive:
                HitPoints = 1;
                Color = new SKColor(255, 100, 0); // Oranye menyala
                break;
            case BrickType.PowerUp:
                HitPoints = 1;
                Color = new SKColor(0, 255, 150); // Hijau neon
                break;
        }

        MaxHitPoints = HitPoints;
        _originalColor = Color;
    }

    /// <summary>
    /// Pukul brick. Mengurangi hit point dan cek apakah hancur.
    /// </summary>
    public void Hit()
    {
        if (IsDestroyed) return;

        HitPoints--;
        _isHitAnimating = true;
        _hitAnimTimer = 0;

        if (HitPoints <= 0)
        {
            IsDestroyed = true;
        }
        else
        {
            // Semakin rusak, warna semakin pudar
            float healthRatio = (float)HitPoints / MaxHitPoints;
            Color = new SKColor(
                (byte)(_originalColor.Red * healthRatio + 50 * (1 - healthRatio)),
                (byte)(_originalColor.Green * healthRatio + 50 * (1 - healthRatio)),
                (byte)(_originalColor.Blue * healthRatio + 50 * (1 - healthRatio)));
        }
    }

    /// <summary>
    /// Hancurkan brick langsung (untuk explosive).
    /// </summary>
    public void Destroy()
    {
        IsDestroyed = true;
        HitPoints = 0;
    }

    /// <summary>
    /// Update animasi hit.
    /// </summary>
    public void Update(float dt)
    {
        if (_isHitAnimating)
        {
            _hitAnimTimer += dt;
            if (_hitAnimTimer > 0.15f)
            {
                _isHitAnimating = false;
            }
        }
    }

    /// <summary>
    /// Render brick dengan efek neon.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        if (IsDestroyed) return;

        float cornerRadius = 4;

        // Hit animation flash
        if (_isHitAnimating)
        {
            using var flashPaint = new SKPaint
            {
                Color = SKColors.White.WithAlpha(180),
                IsAntialias = true
            };
            canvas.DrawRoundRect(Bounds, cornerRadius, cornerRadius, flashPaint);
            return;
        }

        // Glow luar
        using (var glowPaint = new SKPaint
        {
            Color = Color.WithAlpha(40),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6)
        })
        {
            var glowRect = new SKRect(Bounds.Left - 2, Bounds.Top - 2,
                                      Bounds.Right + 2, Bounds.Bottom + 2);
            canvas.DrawRoundRect(glowRect, cornerRadius + 1, cornerRadius + 1, glowPaint);
        }

        // Body brick
        using (var bodyPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(Bounds.Left, Bounds.Top),
                new SKPoint(Bounds.Left, Bounds.Bottom),
                new[] { Color.WithAlpha(255), Color.WithAlpha(180) },
                SKShaderTileMode.Clamp)
        })
        {
            canvas.DrawRoundRect(Bounds, cornerRadius, cornerRadius, bodyPaint);
        }

        // Border
        using (var borderPaint = new SKPaint
        {
            Color = Color.WithAlpha(200),
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f
        })
        {
            canvas.DrawRoundRect(Bounds, cornerRadius, cornerRadius, borderPaint);
        }

        // Highlight atas
        using (var hlPaint = new SKPaint
        {
            Color = SKColors.White.WithAlpha(60),
            IsAntialias = true
        })
        {
            var hlRect = new SKRect(Bounds.Left + 3, Bounds.Top + 2,
                                    Bounds.Right - 3, Bounds.Top + 5);
            canvas.DrawRoundRect(hlRect, 2, 2, hlPaint);
        }

        // Multi-hit: tampilkan indikator sisa hit
        if (BrickType == BrickType.MultiHit && HitPoints > 0)
        {
            using var textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = Bounds.Height * 0.5f,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText(HitPoints.ToString(),
                Bounds.MidX, Bounds.MidY + Bounds.Height * 0.2f, textPaint);
        }

        // Explosive: tanda peringatan
        if (BrickType == BrickType.Explosive)
        {
            using var xPaint = new SKPaint
            {
                Color = SKColors.White.WithAlpha(150),
                IsAntialias = true,
                StrokeWidth = 2,
                Style = SKPaintStyle.Stroke
            };
            float cx = Bounds.MidX, cy = Bounds.MidY;
            float s = Bounds.Width * 0.2f;
            canvas.DrawLine(cx - s, cy - s, cx + s, cy + s, xPaint);
            canvas.DrawLine(cx + s, cy - s, cx - s, cy + s, xPaint);
        }

        // Power-up: tanda bintang
        if (BrickType == BrickType.PowerUp)
        {
            using var starPaint = new SKPaint
            {
                Color = SKColors.White.WithAlpha(200),
                IsAntialias = true,
                TextSize = Bounds.Height * 0.5f,
                TextAlign = SKTextAlign.Center
            };
            canvas.DrawText("✦", Bounds.MidX, Bounds.MidY + Bounds.Height * 0.2f, starPaint);
        }
    }
}
