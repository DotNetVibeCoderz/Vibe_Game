using MarioCart3D.Models;

namespace MarioCart3D.Services;

/// <summary>
/// Singleton service that holds the current game state and player profile.
/// </summary>
public class GameStateService
{
    public GameSession CurrentSession { get; set; } = new();
    public bool IsPaused { get; set; } = false;
    public DateTime RaceStartTime { get; set; }
    public TimeSpan ElapsedTime { get; set; }

    public void StartNewSession(GameMode mode)
    {
        CurrentSession = new GameSession { Mode = mode };
    }

    public void StartRace()
    {
        RaceStartTime = DateTime.UtcNow;
        ElapsedTime = TimeSpan.Zero;
        IsPaused = false;
    }

    public void UpdateElapsedTime()
    {
        if (!IsPaused)
        {
            ElapsedTime = DateTime.UtcNow - RaceStartTime;
        }
    }
}
