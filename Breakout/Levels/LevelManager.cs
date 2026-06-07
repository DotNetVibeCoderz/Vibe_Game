using Breakout.Core;

namespace Breakout.Levels;

/// <summary>
/// Mengelola level-level dalam game.
/// Level bisa dimuat dari file JSON atau dibuat secara prosedural.
/// </summary>
public class LevelManager
{
    private readonly List<LevelData> _levels = new();
    private int _proceduralLevelCount = 0;

    // Konstanta layout
    private const float BRICK_WIDTH = 60;
    private const float BRICK_HEIGHT = 25;
    private const float BRICK_MARGIN = 4;
    private const float GRID_START_X = 40;
    private const float GRID_START_Y = 50;
    private const float PLAY_WIDTH = 720; // Game width - 2*margin

    public LevelManager()
    {
        GenerateProceduralLevels(30); // 30 level
    }

    /// <summary>
    /// Dapatkan data level berdasarkan nomor level.
    /// Jika level > total, generate level prosedural baru.
    /// </summary>
    public LevelData? GetLevel(int levelNumber)
    {
        if (levelNumber <= _levels.Count)
        {
            return _levels[levelNumber - 1];
        }

        // Generate level prosedural baru
        var level = GenerateLevel(levelNumber);
        _levels.Add(level);
        return level;
    }

    /// <summary>
    /// Generate level-level prosedural awal.
    /// </summary>
    private void GenerateProceduralLevels(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            _levels.Add(GenerateLevel(i));
        }
    }

    /// <summary>
    /// Generate satu level berdasarkan nomor.
    /// </summary>
    private LevelData GenerateLevel(int levelNumber)
    {
        var level = new LevelData
        {
            LevelNumber = levelNumber,
            Name = $"Level {levelNumber}",
            Description = GetLevelDescription(levelNumber),
            BallSpeedMultiplier = 1.0f + (levelNumber - 1) * 0.05f
        };

        // Hitung jumlah baris dan kolom
        int cols = 10;
        int rows = Math.Min(3 + levelNumber / 2, 8);

        // Mulai dari level 5, tambah kolom
        if (levelNumber >= 5) cols = 11;
        if (levelNumber >= 10) cols = 12;

        float totalWidth = cols * BRICK_WIDTH + (cols - 1) * BRICK_MARGIN;
        float startX = (800 - totalWidth) / 2;

        var random = new Random(levelNumber * 12345);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                float x = startX + col * (BRICK_WIDTH + BRICK_MARGIN);
                float y = GRID_START_Y + row * (BRICK_HEIGHT + BRICK_MARGIN);

                var type = GetBrickType(levelNumber, row, col, random);

                level.Bricks.Add(new BrickConfig
                {
                    X = x,
                    Y = y,
                    Width = BRICK_WIDTH,
                    Height = BRICK_HEIGHT,
                    Row = row,
                    Column = col,
                    Type = type
                });
            }
        }

        return level;
    }

    /// <summary>
    /// Tentukan tipe brick berdasarkan level dan posisi.
    /// </summary>
    private BrickType GetBrickType(int levelNumber, int row, int col, Random random)
    {
        // Level 1-3: semua standard
        if (levelNumber <= 3)
            return BrickType.Standard;

        // Semakin tinggi level, semakin banyak variasi
        float chance = random.NextSingle();

        // Multi-hit bricks muncul dari level 4
        if (levelNumber >= 4 && chance < 0.1f + (levelNumber * 0.005f))
            return BrickType.MultiHit;

        // Explosive bricks dari level 6
        if (levelNumber >= 6 && chance < 0.05f + (levelNumber * 0.003f))
            return BrickType.Explosive;

        // Power-up bricks dari level 3
        if (levelNumber >= 3 && chance < 0.08f)
            return BrickType.PowerUp;

        return BrickType.Standard;
    }

    /// <summary>
    /// Dapatkan deskripsi level.
    /// </summary>
    private string GetLevelDescription(int levelNumber)
    {
        return levelNumber switch
        {
            1 => "Get Started!",
            2 => "Warming Up",
            3 => "Getting Tougher",
            4 => "Multi-Hit Introduced!",
            5 => "Speed Increases",
            6 => "Explosive Bricks!",
            7 => "The Gauntlet",
            8 => "Brick Fortress",
            9 => "Almost There",
            10 => "Double Digits!",
            11 => "Power-Up Paradise",
            12 => "Speed Demon",
            _ => $"Challenge {levelNumber}"
        };
    }
}
