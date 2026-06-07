using Breakout.Core;
using Breakout.GameObjects;
using SkiaSharp;

namespace Breakout.Core;

/// <summary>
/// Mengelola rendering dan logika menu: Main Menu, Settings, About, Pause, Game Over.
/// </summary>
public class MenuManager
{
    private readonly GameEngine _engine;
    private float _menuAnimTimer;
    private int _selectedMenuIndex;
    private int _selectedSettingsIndex;
    private int _selectedDifficultyIndex;

    // Menu items
    private readonly string[] _mainMenuItems = {
        "New Game",
        "Settings",
        "About",
        "Exit"
    };

    private readonly string[] _settingsItems = {
        $"Difficulty: {GetDifficultyName(DifficultyLevel.Easy)}",
        "Sound: On",
        "Music: On",
        "Show FPS: Off",
        "Ball Trail: On",
        "Particles: On",
        "Back"
    };

    public MenuManager(GameEngine engine)
    {
        _engine = engine;
        SyncSettingsDisplay();
    }

    private void SyncSettingsDisplay()
    {
        var s = _engine.Settings;
        _settingsItems[0] = $"Difficulty: {GetDifficultyName(s.Difficulty)}";
        _settingsItems[1] = $"Sound: {(s.SoundEnabled ? "On" : "Off")}";
        _settingsItems[2] = $"Music: {(s.MusicEnabled ? "On" : "Off")}";
        _settingsItems[3] = $"Show FPS: {(s.ShowFPS ? "On" : "Off")}";
        _settingsItems[4] = $"Ball Trail: {(s.BallTrailEnabled ? "On" : "Off")}";
        _settingsItems[5] = $"Particles: {(s.ParticlesEnabled ? "On" : "Off")}";
    }

    public void Update(float dt)
    {
        _menuAnimTimer += dt;

        // Navigasi menu
        var lastKey = _engine.Input.LastKeyPressed;

        if (_engine.State == GameState.Menu)
        {
            HandleMenuNavigation();
        }
        else if (_engine.State == GameState.Settings)
        {
            HandleSettingsNavigation();
        }
        else if (_engine.State == GameState.About)
        {
            if (_engine.Input.IsKeyPressed(Keys.Escape) || _engine.Input.IsKeyPressed(Keys.Enter) || _engine.Input.IsKeyPressed(Keys.Space))
                _engine.SetState(GameState.Menu);
        }
    }

    private void HandleMenuNavigation()
    {
        if (_engine.Input.IsKeyPressed(Keys.Up) || _engine.Input.IsKeyPressed(Keys.W))
        {
            _selectedMenuIndex = (_selectedMenuIndex - 1 + _mainMenuItems.Length) % _mainMenuItems.Length;
        }
        if (_engine.Input.IsKeyPressed(Keys.Down) || _engine.Input.IsKeyPressed(Keys.S))
        {
            _selectedMenuIndex = (_selectedMenuIndex + 1) % _mainMenuItems.Length;
        }
        if (_engine.Input.IsKeyPressed(Keys.Enter) || _engine.Input.IsKeyPressed(Keys.Space))
        {
            ExecuteMenuAction(_selectedMenuIndex);
        }
    }

    private void ExecuteMenuAction(int index)
    {
        switch (index)
        {
            case 0: // New Game
                _engine.StartNewGame();
                break;
            case 1: // Settings
                _engine.SetState(GameState.Settings);
                _selectedSettingsIndex = 0;
                SyncSettingsDisplay();
                break;
            case 2: // About
                _engine.SetState(GameState.About);
                break;
            case 3: // Exit
                Environment.Exit(0);
                break;
        }
    }

    private void HandleSettingsNavigation()
    {
        if (_engine.Input.IsKeyPressed(Keys.Up) || _engine.Input.IsKeyPressed(Keys.W))
        {
            _selectedSettingsIndex = (_selectedSettingsIndex - 1 + _settingsItems.Length) % _settingsItems.Length;
        }
        if (_engine.Input.IsKeyPressed(Keys.Down) || _engine.Input.IsKeyPressed(Keys.S))
        {
            _selectedSettingsIndex = (_selectedSettingsIndex + 1) % _settingsItems.Length;
        }
        if (_engine.Input.IsKeyPressed(Keys.Left) || _engine.Input.IsKeyPressed(Keys.Right) ||
            _engine.Input.IsKeyPressed(Keys.Enter) || _engine.Input.IsKeyPressed(Keys.Space))
        {
            ToggleSetting(_selectedSettingsIndex);
        }
        if (_engine.Input.IsKeyPressed(Keys.Escape))
        {
            _engine.SetState(GameState.Menu);
        }
    }

    private void ToggleSetting(int index)
    {
        var s = _engine.Settings;
        switch (index)
        {
            case 0: // Difficulty
                s.Difficulty = s.Difficulty switch
                {
                    DifficultyLevel.Easy => DifficultyLevel.Medium,
                    DifficultyLevel.Medium => DifficultyLevel.Hard,
                    DifficultyLevel.Hard => DifficultyLevel.Easy,
                    _ => DifficultyLevel.Medium
                };
                break;
            case 1: s.SoundEnabled = !s.SoundEnabled; break;
            case 2: s.MusicEnabled = !s.MusicEnabled; break;
            case 3: s.ShowFPS = !s.ShowFPS; break;
            case 4: s.BallTrailEnabled = !s.BallTrailEnabled; break;
            case 5: s.ParticlesEnabled = !s.ParticlesEnabled; break;
            case 6: // Back
                _engine.SetState(GameState.Menu);
                s.Save();
                return;
        }
        s.Save();
        SyncSettingsDisplay();
    }

    public void RenderMenu(SKCanvas canvas)
    {
        // Background
        using var bgPaint = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(400, 300), 500,
                new[] { new SKColor(20, 10, 40), SKColors.Black },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, 800, 600, bgPaint);

        // Animated particles background
        float t = _menuAnimTimer;
        for (int i = 0; i < 20; i++)
        {
            float px = 400 + MathF.Sin(t * 0.5f + i * 1.7f) * 200;
            float py = 300 + MathF.Cos(t * 0.3f + i * 2.3f) * 150;
            float alpha = 20 + 15 * MathF.Sin(t + i);
            using var pPaint = new SKPaint
            {
                Color = new SKColor(100, 50, 255, (byte)alpha),
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
            };
            canvas.DrawCircle(px, py, 20 + 10 * MathF.Sin(t * 0.7f + i), pPaint);
        }

        // Title
        float titlePulse = 1.0f + 0.03f * MathF.Sin(t * 2);
        using (var titlePaint = new SKPaint
        {
            IsAntialias = true,
            TextSize = 64 * titlePulse,
            TextAlign = SKTextAlign.Center,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0), new SKPoint(200, 0),
                new[] { new SKColor(255, 0, 128), new SKColor(0, 200, 255) },
                SKShaderTileMode.Clamp)
        })
        {
            // Glow
            using var glowPaint = new SKPaint
            {
                IsAntialias = true,
                TextSize = 64 * titlePulse,
                TextAlign = SKTextAlign.Center,
                Color = new SKColor(100, 0, 200, 80),
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 20)
            };
            canvas.DrawText("BREAKOUT", 402, 122, glowPaint);
            canvas.DrawText("BREAKOUT", 400, 120, titlePaint);
        }

        // Subtitle
        using (var subPaint = new SKPaint
        {
            Color = new SKColor(150, 150, 200, 180),
            TextSize = 16,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        })
        {
            canvas.DrawText("Neon Edition", 400, 155, subPaint);
        }

        // Menu items
        float startY = 250;
        float spacing = 50;

        for (int i = 0; i < _mainMenuItems.Length; i++)
        {
            bool isSelected = i == _selectedMenuIndex;
            float y = startY + i * spacing;

            // Selection glow
            if (isSelected)
            {
                using var selGlow = new SKPaint
                {
                    Color = new SKColor(0, 200, 255, 30),
                    MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10)
                };
                canvas.DrawRoundRect(250, y - 20, 300, 40, 8, 8, selGlow);
            }

            using var textPaint = new SKPaint
            {
                Color = isSelected ? new SKColor(0, 255, 255) : new SKColor(200, 200, 200),
                TextSize = isSelected ? 28 : 24,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };

            if (isSelected)
            {
                // Animated arrow
                float arrowOffset = 10 * MathF.Sin(_menuAnimTimer * 4);
                using var arrowPaint = new SKPaint
                {
                    Color = new SKColor(0, 255, 255),
                    TextSize = 24,
                    TextAlign = SKTextAlign.Center,
                    IsAntialias = true
                };
                canvas.DrawText("▶", 250 - 30 + arrowOffset, y + 8, arrowPaint);
            }

            canvas.DrawText(_mainMenuItems[i], 400, y + 8, textPaint);
        }

        // Footer
        using (var footerPaint = new SKPaint
        {
            Color = new SKColor(100, 100, 150, 100),
            TextSize = 12,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        })
        {
            canvas.DrawText("Use ↑↓ to navigate • Enter/Space to select • Made with SkiaSharp", 400, 570, footerPaint);
        }
    }

    public void RenderSettings(SKCanvas canvas)
    {
        // Background
        using var bgPaint = new SKPaint
        {
            Color = new SKColor(10, 5, 30)
        };
        canvas.DrawRect(0, 0, 800, 600, bgPaint);

        // Title
        using var titlePaint = new SKPaint
        {
            Color = new SKColor(0, 200, 255),
            TextSize = 36,
            TextAlign = SKTextAlign.Left,
            IsAntialias = true
        };
        canvas.DrawText("⚙ Settings", 50, 60, titlePaint);

        // Settings items
        float startY = 120;
        float spacing = 50;

        for (int i = 0; i < _settingsItems.Length; i++)
        {
            bool isSelected = i == _selectedSettingsIndex;
            float y = startY + i * spacing;

            // Selection highlight
            if (isSelected)
            {
                using var selBg = new SKPaint
                {
                    Color = new SKColor(0, 100, 200, 40)
                };
                canvas.DrawRoundRect(40, y - 18, 720, 40, 6, 6, selBg);
            }

            using var textPaint = new SKPaint
            {
                Color = isSelected ? new SKColor(0, 255, 255) : new SKColor(200, 200, 200),
                TextSize = 22,
                TextAlign = SKTextAlign.Left,
                IsAntialias = true
            };

            if (isSelected)
            {
                using var arrowPaint = new SKPaint
                {
                    Color = new SKColor(0, 255, 255),
                    TextSize = 20,
                    TextAlign = SKTextAlign.Left,
                    IsAntialias = true
                };
                canvas.DrawText("▶", 50, y + 7, arrowPaint);
                canvas.DrawText(_settingsItems[i], 80, y + 7, textPaint);
            }
            else
            {
                canvas.DrawText(_settingsItems[i], 70, y + 7, textPaint);
            }

            // Setting value style
            if (!isSelected && i < 6)
            {
                string value = _settingsItems[i].Split(": ").LastOrDefault() ?? "";
                using var valPaint = new SKPaint
                {
                    Color = value == "On" || value == "Easy"
                        ? new SKColor(0, 255, 100)
                        : value == "Off" || value == "Hard"
                            ? new SKColor(255, 100, 100)
                            : new SKColor(255, 200, 0),
                    TextSize = 22,
                    TextAlign = SKTextAlign.Right,
                    IsAntialias = true
                };
                canvas.DrawText(value, 720, y + 7, valPaint);
            }
        }

        // Navigation hint
        using var hintPaint = new SKPaint
        {
            Color = new SKColor(100, 100, 150, 150),
            TextSize = 14,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("↑↓ Navigate • ←→ Toggle • ESC Back", 400, 570, hintPaint);
    }

    public void RenderAbout(SKCanvas canvas)
    {
        // Background
        using var bgPaint = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(400, 300), 400,
                new[] { new SKColor(20, 10, 40), SKColors.Black },
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, 800, 600, bgPaint);

        // Title
        using var titlePaint = new SKPaint
        {
            Color = new SKColor(0, 200, 255),
            TextSize = 36,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("About Breakout", 400, 80, titlePaint);

        // Info
        using var infoPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200),
            TextSize = 18,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };

        string[] aboutLines = {
            "Breakout - Neon Edition",
            "",
            "A classic Breakout game reimagined with",
            "modern neon visuals and particle effects.",
            "",
            "Built with .NET 10 + SkiaSharp",
            "",
            "Features:",
            "• 30+ progressively challenging levels",
            "• 4 brick types: Standard, Multi-Hit, Explosive, Power-Up",
            "• 5 power-ups: Paddle Size, Multi-Ball, Laser, Slow-Motion",
            "• Particle effects & dynamic backgrounds",
            "• 3 difficulty levels: Easy, Medium, Hard",
            "• Built-in level editor",
            "",
            "Developed by Jacky the Code Bender",
            "Gravicode Studios",
            "",
            "Press ESC, ENTER, or SPACE to return"
        };

        float y = 130;
        foreach (var line in aboutLines)
        {
            using var linePaint = new SKPaint
            {
                Color = line.StartsWith("•") ? new SKColor(150, 200, 255) : new SKColor(200, 200, 200),
                TextSize = line.StartsWith("•") ? 16 : 18,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };
            canvas.DrawText(line, 400, y, linePaint);
            y += 28;
        }
    }

    public void RenderPauseOverlay(SKCanvas canvas)
    {
        // Overlay semi-transparan
        using var overlay = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 150)
        };
        canvas.DrawRect(0, 0, 800, 600, overlay);

        using var textPaint = new SKPaint
        {
            Color = new SKColor(0, 200, 255),
            TextSize = 48,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("PAUSED", 400, 280, textPaint);

        using var hintPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200),
            TextSize = 20,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("Press ESC or P to resume", 400, 330, hintPaint);
    }

    public void RenderLevelClear(SKCanvas canvas)
    {
        // Overlay transparan
        using var overlay = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 100)
        };
        canvas.DrawRect(0, 0, 800, 600, overlay);

        float pulse = 1.0f + 0.05f * MathF.Sin(_menuAnimTimer * 3);

        using var textPaint = new SKPaint
        {
            Color = new SKColor(0, 255, 150),
            TextSize = 48 * pulse,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText($"LEVEL {_engine.ScoreManager.Level} CLEAR!", 400, 250, textPaint);

        using var scorePaint = new SKPaint
        {
            Color = new SKColor(255, 255, 200),
            TextSize = 24,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText($"Score: {_engine.ScoreManager.Score}", 400, 300, scorePaint);

        using var hintPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200, 180),
            TextSize = 18,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("Press SPACE or ENTER for next level", 400, 360, hintPaint);
    }

    public void RenderGameOver(SKCanvas canvas)
    {
        // Overlay
        using var overlay = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 180)
        };
        canvas.DrawRect(0, 0, 800, 600, overlay);

        float pulse = 1.0f + 0.03f * MathF.Sin(_menuAnimTimer * 2);

        using var gameOverPaint = new SKPaint
        {
            Color = new SKColor(255, 50, 50),
            TextSize = 56 * pulse,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("GAME OVER", 400, 230, gameOverPaint);

        using var scorePaint = new SKPaint
        {
            Color = new SKColor(255, 255, 200),
            TextSize = 24,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText($"Final Score: {_engine.ScoreManager.Score}", 400, 290, scorePaint);
        canvas.DrawText($"High Score: {_engine.ScoreManager.HighScore}", 400, 320, scorePaint);

        using var hintPaint = new SKPaint
        {
            Color = new SKColor(200, 200, 200, 180),
            TextSize = 18,
            TextAlign = SKTextAlign.Center,
            IsAntialias = true
        };
        canvas.DrawText("Press SPACE or ENTER for main menu", 400, 380, hintPaint);
    }

    private static string GetDifficultyName(DifficultyLevel d) => d switch
    {
        DifficultyLevel.Easy => "Easy",
        DifficultyLevel.Medium => "Medium",
        DifficultyLevel.Hard => "Hard",
        _ => "Medium"
    };
}
