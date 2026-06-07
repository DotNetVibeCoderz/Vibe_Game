using Breakout.Core;
using Breakout.GameObjects;
using SkiaSharp;

namespace Breakout.Levels;

/// <summary>
/// Data untuk satu level. Menyimpan konfigurasi brick layout.
/// </summary>
public class LevelData
{
    public int LevelNumber { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public float BallSpeedMultiplier { get; set; } = 1.0f;
    public List<BrickConfig> Bricks { get; set; } = new();

    /// <summary>
    /// Buat objek Brick dari konfigurasi level.
    /// </summary>
    public List<Brick> CreateBricks(float globalSpeedMultiplier)
    {
        var bricks = new List<Brick>();
        foreach (var config in Bricks)
        {
            var brick = new Brick(
                config.X, config.Y,
                config.Width, config.Height,
                config.Type,
                config.Row
            );
            bricks.Add(brick);
        }
        return bricks;
    }
}

/// <summary>
/// Konfigurasi untuk satu brick dalam level.
/// </summary>
public class BrickConfig
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; } = 60;
    public float Height { get; set; } = 25;
    public int Row { get; set; }
    public int Column { get; set; }
    public BrickType Type { get; set; } = BrickType.Standard;
}
