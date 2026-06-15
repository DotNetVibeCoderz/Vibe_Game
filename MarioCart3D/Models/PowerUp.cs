namespace MarioCart3D.Models;

/// <summary>
/// Represents an item/power-up available during races.
/// </summary>
public class PowerUp
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🎁";
    public PowerUpType Type { get; set; } = PowerUpType.Attack;
}

public enum PowerUpType
{
    Attack,
    Defense,
    Speed,
    Trap
}
