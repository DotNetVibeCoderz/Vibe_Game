namespace MiniRace.Helpers;

public class GameStateData
{
    public int Speed { get; set; }
    public int EnemiesLeft { get; set; }
}

public class GameSettings
{
    public string CarColor { get; set; } = "#FF0000"; // Default Red
    public string Difficulty { get; set; } = "medium";
}
