using System.Text.Json;

namespace Breakout.Core;

/// <summary>
/// Mengelola skor, nyawa, dan high score.
/// </summary>
public class ScoreManager
{
    public int Score { get; private set; }
    public int Lives { get; set; }
    public int Level { get; set; } = 1;
    public int Combo { get; private set; }
    public int HighScore { get; private set; }

    private static readonly string HighScorePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "BreakoutGame", "highscore.json");

    public ScoreManager(int startingLives)
    {
        Lives = startingLives;
        LoadHighScore();
    }

    public void AddScore(int points)
    {
        Combo++;
        // Bonus combo multiplier
        float comboMultiplier = 1.0f + (Combo - 1) * 0.1f;
        if (comboMultiplier > 3.0f) comboMultiplier = 3.0f;

        int finalPoints = (int)(points * comboMultiplier);
        Score += finalPoints;

        if (Score > HighScore)
        {
            HighScore = Score;
            SaveHighScore();
        }
    }

    public void ResetCombo() => Combo = 0;

    public bool LoseLife()
    {
        Lives--;
        ResetCombo();
        return Lives <= 0;
    }

    public void Reset(int startingLives)
    {
        Score = 0;
        Lives = startingLives;
        Combo = 0;
        Level = 1;
    }

    private void LoadHighScore()
    {
        try
        {
            if (File.Exists(HighScorePath))
            {
                var json = File.ReadAllText(HighScorePath);
                HighScore = JsonSerializer.Deserialize<int>(json);
            }
        }
        catch { HighScore = 0; }
    }

    private void SaveHighScore()
    {
        try
        {
            var dir = Path.GetDirectoryName(HighScorePath);
            if (dir != null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(HighScorePath, JsonSerializer.Serialize(HighScore));
        }
        catch { }
    }

    /// <summary>
    /// Poin untuk setiap tipe brick.
    /// </summary>
    public static int PointsForBrick(BrickType type) => type switch
    {
        BrickType.Standard => 10,
        BrickType.MultiHit => 25,
        BrickType.Explosive => 50,
        BrickType.PowerUp => 75,
        _ => 10
    };
}

public enum BrickType
{
    Standard,
    MultiHit,
    Explosive,
    PowerUp
}
