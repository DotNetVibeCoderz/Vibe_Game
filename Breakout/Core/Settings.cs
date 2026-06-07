using System.Text.Json;

namespace Breakout.Core;

/// <summary>
/// Menyimpan semua pengaturan game (difficulty, sound, dll).
/// Disimpan dan dimuat dari file JSON.
/// </summary>
public class AppSettings
{
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
    public bool SoundEnabled { get; set; } = true;
    public bool MusicEnabled { get; set; } = true;
    public bool ShowFPS { get; set; } = false;
    public bool BallTrailEnabled { get; set; } = true;
    public bool ParticlesEnabled { get; set; } = true;
    public int MasterVolume { get; set; } = 80;

    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BreakoutGame", "settings.json");

    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { /* gunakan default */ }
        return new AppSettings();
    }

    /// <summary>
    /// Mendapatkan multiplier kecepatan berdasarkan difficulty.
    /// </summary>
    public float SpeedMultiplier => Difficulty switch
    {
        DifficultyLevel.Easy => 0.7f,
        DifficultyLevel.Medium => 1.0f,
        DifficultyLevel.Hard => 1.3f,
        _ => 1.0f
    };

    /// <summary>
    /// Mendapatkan jumlah nyawa awal berdasarkan difficulty.
    /// </summary>
    public int StartingLives => Difficulty switch
    {
        DifficultyLevel.Easy => 5,
        DifficultyLevel.Medium => 3,
        DifficultyLevel.Hard => 2,
        _ => 3
    };
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}
