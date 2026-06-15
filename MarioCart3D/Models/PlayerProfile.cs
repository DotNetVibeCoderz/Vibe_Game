namespace MarioCart3D.Models;

/// <summary>
/// Stores player progress, unlocks, and statistics.
/// </summary>
public class PlayerProfile
{
    public string PlayerName { get; set; } = "Player 1";
    public int TotalCoins { get; set; } = 0;
    public int RacesWon { get; set; } = 0;
    public int RacesPlayed { get; set; } = 0;
    public int BestTimeMs { get; set; } = int.MaxValue;

    // StreetPass ranking style
    public int StreetPassScore { get; set; } = 0;
    public int GlobalRank { get; set; } = 9999;

    public List<string> UnlockedCharacters { get; set; } = new();
    public List<string> UnlockedKarts { get; set; } = new();
    public List<string> UnlockedTracks { get; set; } = new();
}
