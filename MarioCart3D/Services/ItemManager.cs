using MarioCart3D.Models;

namespace MarioCart3D.Services;

/// <summary>
/// Manages item/power-up usage during races.
/// </summary>
public class ItemManager
{
    private readonly GameDataService _gameData;
    private readonly Random _random = new();

    public PowerUp? CurrentItem { get; private set; }

    public ItemManager(GameDataService gameData)
    {
        _gameData = gameData;
    }

    public void RollItemBox()
    {
        var items = _gameData.GetPowerUps();
        int index = _random.Next(items.Count);
        CurrentItem = items[index];
    }

    public void UseItem()
    {
        CurrentItem = null;
    }

    public void ClearItem()
    {
        CurrentItem = null;
    }
}
