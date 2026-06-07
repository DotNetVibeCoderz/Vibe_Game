using SkiaSharp;

namespace Breakout.Effects;

/// <summary>
/// Latar belakang dinamis dengan tema berbeda per level.
/// Mendukung efek parallax dan animasi.
/// </summary>
public class Background
{
    private float _scrollOffset;
    private float _starOffset;
    private float _parallaxOffset;

    // Tema latar belakang
    private BackgroundTheme _currentTheme = BackgroundTheme.Space;
    private float _themeTransition;
    private bool _isTransitioning;

    // Bintang untuk tema space
    private readonly Star[] _stars;

    // Grid neon untuk tema neon
    private readonly Random _random = new();

    public enum BackgroundTheme
    {
        Space,
        Neon,
        RetroArcade
    }

    public Background()
    {
        // Generate stars
        _stars = new Star[100];
        for (int i = 0; i < _stars.Length; i++)
        {
            _stars[i] = new Star
            {
                X = (float)(_random.NextDouble() * 100),
                Y = (float)(_random.NextDouble() * 100),
                Size = 0.5f + (float)_random.NextDouble() * 2,
                Brightness = 0.3f + (float)_random.NextDouble() * 0.7f,
                Speed = 0.5f + (float)_random.NextDouble() * 2
            };
        }
    }

    /// <summary>
    /// Set tema berdasarkan level.
    /// </summary>
    public void SetLevelTheme(int level)
    {
        var newTheme = (level % 3) switch
        {
            0 => BackgroundTheme.Space,
            1 => BackgroundTheme.Neon,
            2 => BackgroundTheme.RetroArcade,
            _ => BackgroundTheme.Space
        };

        if (newTheme != _currentTheme)
        {
            _currentTheme = newTheme;
            _isTransitioning = true;
            _themeTransition = 0;
        }
    }

    /// <summary>
    /// Update animasi background.
    /// </summary>
    public void Update(float dt)
    {
        _scrollOffset += dt * 30;
        _starOffset += dt * 20;
        _parallaxOffset += dt * 10;

        if (_isTransitioning)
        {
            _themeTransition += dt;
            if (_themeTransition >= 1.0f)
            {
                _themeTransition = 1.0f;
                _isTransitioning = false;
            }
        }
    }

    /// <summary>
    /// Render background.
    /// </summary>
    public void Render(SKCanvas canvas, float width, float height)
    {
        switch (_currentTheme)
        {
            case BackgroundTheme.Space:
                RenderSpaceTheme(canvas, width, height);
                break;
            case BackgroundTheme.Neon:
                RenderNeonTheme(canvas, width, height);
                break;
            case BackgroundTheme.RetroArcade:
                RenderRetroTheme(canvas, width, height);
                break;
        }
    }

    private void RenderSpaceTheme(SKCanvas canvas, float width, float height)
    {
        // Deep space gradient
        using var bgPaint = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(width / 2, height / 2),
                width * 0.7f,
                new[] { new SKColor(10, 5, 40), new SKColor(0, 0, 10), SKColors.Black },
                new float[] { 0, 0.5f, 1 },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        // Nebula effect
        using var nebulaPaint = new SKPaint
        {
            Color = new SKColor(100, 0, 150, 15),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 60)
        };
        canvas.DrawCircle(
            width * 0.3f + MathF.Sin(_scrollOffset * 0.01f) * 50,
            height * 0.4f + MathF.Cos(_scrollOffset * 0.015f) * 30,
            120, nebulaPaint);

        using var nebulaPaint2 = new SKPaint
        {
            Color = new SKColor(0, 50, 150, 10),
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 80)
        };
        canvas.DrawCircle(
            width * 0.7f + MathF.Sin(_scrollOffset * 0.02f) * 40,
            height * 0.6f + MathF.Cos(_scrollOffset * 0.01f) * 40,
            150, nebulaPaint2);

        // Stars
        foreach (var star in _stars)
        {
            float sx = star.X * width / 100;
            float sy = (star.Y * height / 100 + _starOffset * star.Speed) % height;
            float brightness = star.Brightness * (0.7f + 0.3f * MathF.Sin(_scrollOffset * 0.05f + star.X));

            using var starPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, (byte)(brightness * 255)),
                IsAntialias = true
            };
            canvas.DrawCircle(sx, sy, star.Size, starPaint);
        }
    }

    private void RenderNeonTheme(SKCanvas canvas, float width, float height)
    {
        // Dark background
        using var bgPaint = new SKPaint { Color = new SKColor(5, 5, 20) };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        // Neon grid
        float gridSize = 40;
        using var gridPaint = new SKPaint
        {
            Color = new SKColor(0, 200, 255, 20),
            IsAntialias = true,
            StrokeWidth = 0.5f
        };

        float offsetY = _scrollOffset % gridSize;

        for (float y = -gridSize + offsetY; y < height + gridSize; y += gridSize)
        {
            canvas.DrawLine(0, y, width, y, gridPaint);
        }

        for (float x = 0; x < width; x += gridSize)
        {
            canvas.DrawLine(x, 0, x, height, gridPaint);
        }

        // Neon glow dots at intersections
        using var dotPaint = new SKPaint
        {
            Color = new SKColor(255, 0, 128, 30),
            IsAntialias = true
        };
        for (float y = -gridSize + offsetY; y < height + gridSize; y += gridSize)
        {
            for (float x = 0; x < width; x += gridSize)
            {
                canvas.DrawCircle(x, y, 2, dotPaint);
            }
        }

        // Moving neon line
        using var linePaint = new SKPaint
        {
            Color = new SKColor(255, 0, 128, 40),
            IsAntialias = true,
            StrokeWidth = 2,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8)
        };
        float lineY = (_scrollOffset * 2) % height;
        canvas.DrawLine(0, lineY, width, lineY, linePaint);
    }

    private void RenderRetroTheme(SKCanvas canvas, float width, float height)
    {
        // Retro arcade gradient
        using var bgPaint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0), new SKPoint(0, height),
                new[] { new SKColor(10, 5, 20), new SKColor(20, 5, 30), new SKColor(5, 5, 15) },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, width, height, bgPaint);

        // Retro scanlines
        using var scanPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 30),
            StrokeWidth = 1
        };
        for (float y = 0; y < height; y += 3)
        {
            canvas.DrawLine(0, y, width, y, scanPaint);
        }

        // Retro grid dots
        using var dotPaint = new SKPaint
        {
            Color = new SKColor(255, 200, 0, 15),
            IsAntialias = true
        };
        float spacing = 30;
        float offY = _scrollOffset % spacing;
        for (float y = -spacing + offY; y < height + spacing; y += spacing)
        {
            for (float x = 0; x < width; x += spacing)
            {
                canvas.DrawCircle(x, y, 1.5f, dotPaint);
            }
        }

        // Retro rainbow bar at bottom
        using var rainbowPaint = new SKPaint
        {
            IsAntialias = true
        };
        for (int i = 0; i < 3; i++)
        {
            var rc = i switch
            {
                0 => new SKColor(255, 50, 50, 20),
                1 => new SKColor(255, 200, 50, 20),
                2 => new SKColor(50, 255, 50, 20),
                _ => new SKColor(50, 50, 255, 20)
            };
            rainbowPaint.Color = rc;
            canvas.DrawRect(0, height - 3 + i * 1, width, 1, rainbowPaint);
        }
    }

    private class Star
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Size { get; set; }
        public float Brightness { get; set; }
        public float Speed { get; set; }
    }
}
