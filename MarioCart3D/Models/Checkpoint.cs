namespace MarioCart3D.Models;

/// <summary>
/// Represents a checkpoint on the track for lap validation and position calculation.
/// </summary>
public class Checkpoint
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
    public float Radius { get; set; } = 10f;
    public bool IsFinishLine { get; set; } = false;
}
