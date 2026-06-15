namespace MarioCart3D.Models;

/// <summary>
/// Represents wheel/tire selection for kart customization.
/// </summary>
public class Wheel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsUnlocked { get; set; } = true;

    public int SpeedMod { get; set; } = 0;
    public int AccelerationMod { get; set; } = 0;
    public int HandlingMod { get; set; } = 0;
    public int OffRoadMod { get; set; } = 0;
}
