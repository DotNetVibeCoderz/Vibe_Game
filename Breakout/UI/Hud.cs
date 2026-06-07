using Breakout.Core;
using SkiaSharp;

namespace Breakout.Core;

/// <summary>
/// HUD (Heads-Up Display) yang menampilkan informasi game:
/// skor, nyawa, level, indikator power-up.
/// </summary>
public class Hud
{
    private readonly GameEngine _engine;
    private float _comboTimer;
    private int _lastCombo;

    public Hud(GameEngine engine)
    {
        _engine = engine;
    }

    /// <summary>
    /// Render HUD di atas game.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        var score = _engine.ScoreManager;

        // --- Top bar background ---
        using (var topBarPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 120)
        })
        {
            canvas.DrawRect(0, 0, 800, 35, topBarPaint);
        }

        // --- Level ---
        using (var levelPaint = new SKPaint
        {
            Color = new SKColor(0, 200, 255),
            TextSize = 18,
            TextAlign = SKTextAlign.Left,
            IsAntialias = true
        })
        {
            canvas.DrawText($"Level {score.Level}", 15, 24, levelPaint);
        }

        // --- Score ---
        using (var scorePaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 18,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        })
        {
            canvas.DrawText($"Score: {score.Score}", 400, 24, scorePaint);
        }

        // --- High Score ---
        using (var hsPaint = new SKPaint
        {
            Color = new SKColor(255, 200, 50),
            TextSize = 14,
            TextAlign = SKTextAlign.Right,
            IsAntialias = true
        })
        {
            canvas.DrawText($"High: {score.HighScore}", 780, 24, hsPaint);
        }

        // --- Lives (hati) ---
        for (int i = 0; i < score.Lives; i++)
        {
            float x = 600 + i * 25;
            using var heartPaint = new SKPaint
            {
                Color = new SKColor(255, 50, 100),
                TextSize = 18,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };
            canvas.DrawText("♥", x, 25, heartPaint);
        }

        // --- Combo display ---
        if (score.Combo > 1)
        {
            _comboTimer = 0.5f;
            _lastCombo = score.Combo;
        }

        if (_comboTimer > 0)
        {
            _comboTimer -= 0.016f;
            float comboAlpha = Math.Clamp(_comboTimer * 2, 0, 1);

            using var comboPaint = new SKPaint
            {
                Color = new SKColor(255, 200, 50, (byte)(comboAlpha * 255)),
                TextSize = 16,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };
            canvas.DrawText($"Combo x{_lastCombo}", 400, 55, comboPaint);
        }

        // --- Power-up indicators ---
        RenderPowerUpIndicators(canvas);
    }

    /// <summary>
    /// Render indikator power-up aktif di bagian bawah.
    /// </summary>
    private void RenderPowerUpIndicators(SKCanvas canvas)
    {
        // Kita cek timer dari engine
        float y = 570;
        float x = 400;

        // Slow-motion indicator
        if (_engine.GetType().GetField("_slowMotionTimer",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null)
        {
            // We'll just show a power-up indicator area
        }

        // Simplified: just show active power-ups text
        using var infoPaint = new SKPaint
        {
            Color = new SKColor(0, 200, 255, 100),
            TextSize = 12,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("[SPACE] Launch Ball  •  [P] Pause  •  [ESC] Menu", 400, 585, infoPaint);
    }
}
