namespace MarioCart3D.Models;

/// <summary>
/// Enum representing available game modes.
/// </summary>
public enum GameMode
{
    GrandPrix,
    TimeTrial,
    VsRace,
    BattleMode
}

/// <summary>
/// Represents a game session configuration.
/// </summary>
public class GameSession
{
    public GameMode Mode { get; set; } = GameMode.GrandPrix;
    public RaceTrack? Track { get; set; }
    public Racer? PlayerRacer { get; set; }
    public Kart? SelectedKart { get; set; }
    public Wheel? SelectedWheels { get; set; }
    public Glider? SelectedGlider { get; set; }
    public int AiCount { get; set; } = 7;
    public int Laps { get; set; } = 3;
    public bool IsMirrorMode { get; set; } = false;
    public bool Is200cc { get; set; } = false;
}
