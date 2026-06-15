namespace MarioCart3D.Models;

/// <summary>
/// Represents a race track/course in the game.
/// </summary>
public class RaceTrack
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Theme { get; set; } = "Grass";
    public string Difficulty { get; set; } = "Normal";
    public bool IsUnlocked { get; set; } = true;
    public int UnlockRequirement { get; set; } = 0;

    // Visual properties
    public string SkyColor { get; set; } = "#87CEEB";
    public string GroundColor { get; set; } = "#2d5016";
    public string RoadColor { get; set; } = "#444444";

    // Track shape parameters
    public float RadiusX { get; set; } = 40f;
    public float RadiusZ { get; set; } = 25f;
    public int Laps { get; set; } = 3;
}
