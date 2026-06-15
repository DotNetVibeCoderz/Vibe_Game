using MarioCart3D.Models;

namespace MarioCart3D.Services;

/// <summary>
/// Service providing game data: racers, tracks, karts, power-ups, etc.
/// </summary>
public class GameDataService
{
    public List<Racer> GetRacers() => new()
    {
        new Racer { Id = 1, Name = "Mario", Description = "Balanced all-rounder", ColorHex = "#e94560", Speed = 5, Acceleration = 5, Handling = 5, Weight = 5, IsUnlocked = true },
        new Racer { Id = 2, Name = "Luigi", Description = "Slightly better handling", ColorHex = "#2ecc71", Speed = 5, Acceleration = 5, Handling = 6, Weight = 5, IsUnlocked = true },
        new Racer { Id = 3, Name = "Peach", Description = "High acceleration, light", ColorHex = "#ff69b4", Speed = 4, Acceleration = 7, Handling = 6, Weight = 3, IsUnlocked = true },
        new Racer { Id = 4, Name = "Bowser", Description = "Heavy with top speed", ColorHex = "#f39c12", Speed = 7, Acceleration = 3, Handling = 3, Weight = 9, IsUnlocked = false, UnlockRequirement = 3 },
        new Racer { Id = 5, Name = "Yoshi", Description = "Quick off the line", ColorHex = "#9acd32", Speed = 5, Acceleration = 6, Handling = 5, Weight = 4, IsUnlocked = true },
        new Racer { Id = 6, Name = "Toad", Description = "Tiny but nimble", ColorHex = "#3498db", Speed = 3, Acceleration = 8, Handling = 8, Weight = 2, IsUnlocked = true }
    };

    public List<Kart> GetKarts() => new()
    {
        new Kart { Id = 1, Name = "Standard Kart", Description = "Balanced performance", SpeedMod = 0, AccelerationMod = 0, HandlingMod = 0, IsUnlocked = true },
        new Kart { Id = 2, Name = "Pipe Frame", Description = "Light with good acceleration", SpeedMod = -1, AccelerationMod = 2, HandlingMod = 1, WeightMod = -1, IsUnlocked = true },
        new Kart { Id = 3, Name = "B Dasher", Description = "Fast but heavy handling", SpeedMod = 2, AccelerationMod = -1, HandlingMod = -2, WeightMod = 1, IsUnlocked = false, UnlockRequirement = 5 },
        new Kart { Id = 4, Name = "Sneeker", Description = "High speed, lower accel", SpeedMod = 2, AccelerationMod = -2, HandlingMod = 0, WeightMod = 0, IsUnlocked = false, UnlockRequirement = 8 }
    };

    public List<Wheel> GetWheels() => new()
    {
        new Wheel { Id = 1, Name = "Standard", Description = "Balanced tires", SpeedMod = 0, AccelerationMod = 0, HandlingMod = 0, IsUnlocked = true },
        new Wheel { Id = 2, Name = "Roller", Description = "Great acceleration", SpeedMod = -1, AccelerationMod = 2, HandlingMod = 1, IsUnlocked = true },
        new Wheel { Id = 3, Name = "Slick", Description = "Speed on paved roads", SpeedMod = 2, AccelerationMod = -1, HandlingMod = -1, IsUnlocked = true },
        new Wheel { Id = 4, Name = "Off-Road", Description = "Better on dirt and grass", SpeedMod = 0, AccelerationMod = 0, HandlingMod = 0, OffRoadMod = 3, IsUnlocked = true }
    };

    public List<Glider> GetGliders() => new()
    {
        new Glider { Id = 1, Name = "Super Glider", Description = "Balanced glider", AirSpeedMod = 0, AirHandlingMod = 0, IsUnlocked = true },
        new Glider { Id = 2, Name = "Cloud Glider", Description = "Better air handling", AirSpeedMod = -1, AirHandlingMod = 2, IsUnlocked = true },
        new Glider { Id = 3, Name = "Wario Wing", Description = "Fast glider", AirSpeedMod = 2, AirHandlingMod = -1, IsUnlocked = true }
    };

    public List<RaceTrack> GetTracks() => new()
    {
        new RaceTrack { Id = 1, Name = "Mushroom Circuit", Description = "A classic oval circuit with gentle curves.", Theme = "Grassland", Difficulty = "Easy", SkyColor = "#87CEEB", GroundColor = "#2d5016", IsUnlocked = true },
        new RaceTrack { Id = 2, Name = "Bowser's Castle", Description = "Twisty track through a fiery fortress.", Theme = "Castle", Difficulty = "Hard", SkyColor = "#2c0b0b", GroundColor = "#4a2511", RoadColor = "#663333", IsUnlocked = false, UnlockRequirement = 2 },
        new RaceTrack { Id = 3, Name = "Rainbow Road", Description = "Iconic rainbow highway in space.", Theme = "Space", Difficulty = "Expert", SkyColor = "#0a0a2e", GroundColor = "#1a1a3e", RoadColor = "#ff00ff", IsUnlocked = false, UnlockRequirement = 5 },
        new RaceTrack { Id = 4, Name = "Koopa Beach", Description = "Sandy shores with shallow water shortcuts.", Theme = "Beach", Difficulty = "Normal", SkyColor = "#87CEEB", GroundColor = "#e6c288", RoadColor = "#d4a76a", IsUnlocked = true },
        new RaceTrack { Id = 5, Name = "Ghost Valley", Description = "Haunted track with tricky turns.", Theme = "Haunted", Difficulty = "Hard", SkyColor = "#1a1a2e", GroundColor = "#2a2a3e", RoadColor = "#555566", IsUnlocked = false, UnlockRequirement = 4 },
        new RaceTrack { Id = 6, Name = "DK Mountain", Description = "Mountainous terrain with steep drops.", Theme = "Mountain", Difficulty = "Expert", SkyColor = "#87CEEB", GroundColor = "#5a4a3a", RoadColor = "#6b5b4b", IsUnlocked = false, UnlockRequirement = 6 }
    };

    public List<PowerUp> GetPowerUps() => new()
    {
        new PowerUp { Id = 1, Name = "Green Shell", Description = "Travels straight forward", Icon = "🐢", Type = PowerUpType.Attack },
        new PowerUp { Id = 2, Name = "Red Shell", Description = "Homes in on the racer ahead", Icon = "🐚", Type = PowerUpType.Attack },
        new PowerUp { Id = 3, Name = "Banana", Description = "Slip trap for trailing racers", Icon = "🍌", Type = PowerUpType.Trap },
        new PowerUp { Id = 4, Name = "Star", Description = "Temporary invincibility and speed", Icon = "⭐", Type = PowerUpType.Speed },
        new PowerUp { Id = 5, Name = "Bullet Bill", Description = "Auto-pilot rocket boost", Icon = "🚀", Type = PowerUpType.Speed },
        new PowerUp { Id = 6, Name = "Blue Shell", Description = "Targets the race leader", Icon = "💣", Type = PowerUpType.Attack }
    };
}
