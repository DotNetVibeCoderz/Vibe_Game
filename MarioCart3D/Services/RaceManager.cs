using MarioCart3D.Models;

namespace MarioCart3D.Services;

/// <summary>
/// Manages race state: lap counting, checkpoint progression, positions, and finish order.
/// </summary>
public class RaceManager
{
    private readonly List<Checkpoint> _checkpoints = new();
    private readonly List<RacePosition> _positions = new();
    private int _finishCounter = 0;

    public IReadOnlyList<RacePosition> Positions => _positions.AsReadOnly();
    public IReadOnlyList<Checkpoint> Checkpoints => _checkpoints.AsReadOnly();

    public void InitializeTrack(float radiusX, float radiusZ, int checkpointCount = 8)
    {
        _checkpoints.Clear();
        _positions.Clear();
        _finishCounter = 0;

        for (int i = 0; i < checkpointCount; i++)
        {
            float angle = (float)(i * 2 * Math.PI / checkpointCount);
            _checkpoints.Add(new Checkpoint
            {
                Id = i,
                X = (float)(Math.Cos(angle) * radiusX),
                Z = (float)(Math.Sin(angle) * radiusZ),
                Radius = 12f,
                IsFinishLine = i == 0
            });
        }
    }

    public void RegisterRacer(int racerId, string racerName, bool isPlayer)
    {
        _positions.Add(new RacePosition
        {
            RacerId = racerId,
            RacerName = racerName,
            IsPlayer = isPlayer
        });
    }

    public void UpdateRacerPosition(int racerId, float x, float z)
    {
        var pos = _positions.FirstOrDefault(p => p.RacerId == racerId);
        if (pos == null || pos.HasFinished) return;

        // Find nearest checkpoint ahead
        int nextCheckpoint = (pos.CheckpointIndex + 1) % _checkpoints.Count;
        var cp = _checkpoints[nextCheckpoint];
        float dx = x - cp.X;
        float dz = z - cp.Z;
        float dist = (float)Math.Sqrt(dx * dx + dz * dz);

        if (dist < cp.Radius)
        {
            pos.CheckpointIndex = nextCheckpoint;

            if (cp.IsFinishLine)
            {
                pos.Lap++;
                if (pos.Lap > 3) // default 3 laps
                {
                    pos.HasFinished = true;
                    pos.FinishPosition = ++_finishCounter;
                }
            }
        }

        // Calculate total progress for ranking
        pos.TotalProgress = (pos.Lap - 1) * _checkpoints.Count + pos.CheckpointIndex;
    }

    public void RecalculatePositions()
    {
        var ordered = _positions.OrderByDescending(p => p.TotalProgress).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].FinishPosition = ordered[i].HasFinished ? ordered[i].FinishPosition : i + 1;
        }
    }

    public RacePosition? GetPlayerPosition()
    {
        return _positions.FirstOrDefault(p => p.IsPlayer);
    }

    public string GetOrdinalPosition(int position)
    {
        if (position <= 0) return "--";
        if (position % 100 >= 11 && position % 100 <= 13)
            return $"{position}th";
        return (position % 10) switch
        {
            1 => $"{position}st",
            2 => $"{position}nd",
            3 => $"{position}rd",
            _ => $"{position}th"
        };
    }
}
