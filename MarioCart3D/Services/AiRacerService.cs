using MarioCart3D.Models;

namespace MarioCart3D.Services;

/// <summary>
/// Manages AI racers with improved path-following behavior including elevation.
/// </summary>
public class AiRacerService
{
    private readonly List<AiKartState> _aiKarts = new();
    private readonly Random _random = new();

    public IReadOnlyList<AiKartState> AiKarts => _aiKarts.AsReadOnly();

    public void Initialize(int count, float radiusX, float radiusZ, float speedVariation = 0.15f)
    {
        _aiKarts.Clear();

        for (int i = 0; i < count; i++)
        {
            float angle = (float)(i * 2 * Math.PI / Math.Max(count, 1));
            _aiKarts.Add(new AiKartState
            {
                Id = i + 100,
                Name = $"CPU {i + 1}",
                Angle = angle,
                RadiusX = radiusX + (float)(_random.NextDouble() - 0.5) * 6,
                RadiusZ = radiusZ + (float)(_random.NextDouble() - 0.5) * 4,
                BaseSpeed = 0.35f + (float)(_random.NextDouble() * speedVariation),
                Offset = (float)(_random.NextDouble() * Math.PI * 2),
                Wobble = (float)(_random.NextDouble() * 0.08),
                SkillLevel = _random.NextDouble()
            });
        }
    }

    public void Update(float deltaTime)
    {
        foreach (var ai in _aiKarts)
        {
            // Dynamic speed adjustment based on skill
            float speedMultiplier = 1.0f + (float)(ai.SkillLevel * 0.3);
            ai.Angle += ai.BaseSpeed * deltaTime * 0.5f * speedMultiplier;

            // Create interesting path with harmonics matching track generation
            float rX = ai.RadiusX + (float)(Math.Sin(ai.Angle * 3 + ai.Offset) * 5);
            float rZ = ai.RadiusZ + (float)(Math.Cos(ai.Angle * 2 + ai.Offset) * 4);

            ai.PositionX = (float)(Math.Cos(ai.Angle + ai.Offset) * rX);
            ai.PositionZ = (float)(Math.Sin(ai.Angle + ai.Offset) * rZ);

            // Elevation matching track
            ai.PositionY = (float)(Math.Sin((ai.Angle + ai.Offset) * 2) * 3 + Math.Cos((ai.Angle + ai.Offset) * 4) * 2) * 0.15f;

            // Slight wobble for realism
            ai.PositionX += (float)(Math.Sin(ai.Angle * 3) * ai.Wobble);
            ai.PositionZ += (float)(Math.Cos(ai.Angle * 3) * ai.Wobble);

            // Calculate rotation (tangent to path)
            float nextAngle = ai.Angle + 0.05f;
            float nextX = (float)(Math.Cos(nextAngle + ai.Offset) * rX);
            float nextZ = (float)(Math.Sin(nextAngle + ai.Offset) * rZ);
            ai.RotationY = (float)(Math.Atan2(nextZ - ai.PositionZ, nextX - ai.PositionX));
        }
    }
}

public class AiKartState
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float Angle { get; set; }
    public float RotationY { get; set; }
    public float RadiusX { get; set; }
    public float RadiusZ { get; set; }
    public float BaseSpeed { get; set; }
    public float Offset { get; set; }
    public float Wobble { get; set; }
    public double SkillLevel { get; set; }
}
