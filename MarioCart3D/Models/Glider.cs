namespace MarioCart3D.Models;

/// <summary>
/// Represents glider selection for aerial performance.
/// </summary>
public class Glider
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = true;

    public int AirSpeedMod { get; set; } = 0;
    public int AirHandlingMod { get; set; } = 0;
    public int WeightMod { get; set; } = 0;
}
