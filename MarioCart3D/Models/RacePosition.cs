namespace MarioCart3D.Models;

/// <summary>
/// Represents a racer's current race status including lap and checkpoint progress.
/// </summary>
public class RacePosition
{
    public int RacerId { get; set; }
    public string RacerName { get; set; } = string.Empty;
    public int Lap { get; set; } = 1;
    public int CheckpointIndex { get; set; } = 0;
    public float TotalProgress { get; set; } = 0;
    public bool IsPlayer { get; set; } = false;
    public bool HasFinished { get; set; } = false;
    public int FinishPosition { get; set; } = 0;
}
