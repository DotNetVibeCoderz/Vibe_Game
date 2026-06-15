using MarioCart3D.Models;
using System.IO;
using System.Text.Json;

namespace MarioCart3D.Services;

/// <summary>
/// Manages player progression, unlocks, and profile persistence.
/// </summary>
public class ProgressionService
{
    private readonly string _profilePath;
    private readonly GameDataService _gameData;

    public PlayerProfile Profile { get; private set; } = new();

    public ProgressionService(GameDataService gameData)
    {
        _gameData = gameData;
        _profilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MarioCart3D",
            "profile.json");

        LoadProfile();
    }

    public void LoadProfile()
    {
        try
        {
            if (File.Exists(_profilePath))
            {
                var json = File.ReadAllText(_profilePath);
                Profile = JsonSerializer.Deserialize<PlayerProfile>(json) ?? new PlayerProfile();
            }
            else
            {
                Profile = new PlayerProfile();
                SaveProfile();
            }

            if (Profile.UnlockedCharacters.Count == 0)
                Profile.UnlockedCharacters.Add("Mario");
            if (Profile.UnlockedKarts.Count == 0)
                Profile.UnlockedKarts.Add("Standard Kart");
            if (Profile.UnlockedTracks.Count == 0)
                Profile.UnlockedTracks.Add("Mushroom Circuit");
        }
        catch
        {
            Profile = new PlayerProfile();
        }
    }

    public void SaveProfile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_profilePath)!);
            var json = JsonSerializer.Serialize(Profile, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_profilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save profile: {ex.Message}");
        }
    }

    public void AddCoins(int amount)
    {
        Profile.TotalCoins += amount;
        SaveProfile();
    }

    public void RecordRace(bool won, int timeMs)
    {
        Profile.RacesPlayed++;
        if (won)
        {
            Profile.RacesWon++;
            Profile.StreetPassScore += 100;
        }
        else
        {
            Profile.StreetPassScore += 10;
        }

        if (timeMs < Profile.BestTimeMs)
            Profile.BestTimeMs = timeMs;

        Profile.GlobalRank = Math.Max(1, 9999 - Profile.StreetPassScore / 50);
        SaveProfile();
    }

    public void UnlockCharacter(string name)
    {
        if (!Profile.UnlockedCharacters.Contains(name))
        {
            Profile.UnlockedCharacters.Add(name);
            SaveProfile();
        }
    }

    public void UnlockKart(string name)
    {
        if (!Profile.UnlockedKarts.Contains(name))
        {
            Profile.UnlockedKarts.Add(name);
            SaveProfile();
        }
    }

    public void UnlockTrack(string name)
    {
        if (!Profile.UnlockedTracks.Contains(name))
        {
            Profile.UnlockedTracks.Add(name);
            SaveProfile();
        }
    }

    public bool IsUnlocked(string name, UnlockType type)
    {
        return type switch
        {
            UnlockType.Character => Profile.UnlockedCharacters.Contains(name),
            UnlockType.Kart => Profile.UnlockedKarts.Contains(name),
            UnlockType.Track => Profile.UnlockedTracks.Contains(name),
            _ => false
        };
    }
}

public enum UnlockType
{
    Character,
    Kart,
    Track
}
