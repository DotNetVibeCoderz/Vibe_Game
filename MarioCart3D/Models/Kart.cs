namespace MarioCart3D.Models;

/// <summary>
/// Represents a kart body selection with its impact on performance.
/// </summary>
public class Kart
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = true;
    public int UnlockRequirement { get; set; } = 0;

    // Stat modifiers (-3 to +3)
    public int SpeedMod { get; set; } = 0;
    public int AccelerationMod { get; set; } = 0;
    public int HandlingMod { get; set; } = 0;
    public int WeightMod { get; set; } = 0;
}
