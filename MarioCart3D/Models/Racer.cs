namespace MarioCart3D.Models;

/// <summary>
/// Represents a playable racer/character in the game.
/// </summary>
public class Racer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#e94560";
    public bool IsUnlocked { get; set; } = true;
    public int UnlockRequirement { get; set; } = 0;

    // Stats (0-10 scale)
    public int Speed { get; set; } = 5;
    public int Acceleration { get; set; } = 5;
    public int Handling { get; set; } = 5;
    public int Weight { get; set; } = 5;

    public string ImageUrl { get; set; } = string.Empty;
}
