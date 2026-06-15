using Microsoft.JSInterop;

namespace MarioCart3D.Services;

/// <summary>
/// Audio service for background music and sound effects.
/// Stores volume settings; actual audio playback is triggered from JS side.
/// </summary>
public class AudioService
{
    private readonly IJSRuntime? _js;

    public float MasterVolume { get; set; } = 0.8f;
    public float MusicVolume { get; set; } = 0.5f;
    public float SfxVolume { get; set; } = 0.6f;
    public bool IsMuted { get; set; } = false;

    public AudioService(IJSRuntime? js = null)
    {
        _js = js;
    }

    private bool CanUseJS()
    {
        return _js != null;
    }

    public async Task PlayBackgroundMusic(string trackTheme)
    {
        if (!CanUseJS()) return;

        var theme = trackTheme.ToLowerInvariant() switch
        {
            "grassland" => "grassland",
            "castle" => "castle",
            "space" => "space",
            "beach" => "beach",
            "haunted" => "haunted",
            "mountain" => "mountain",
            _ => "grassland"
        };

        try
        {
            await _js!.InvokeVoidAsync("gameEngine.playMusic", theme);
        }
        catch (InvalidOperationException)
        {
            // WebView not ready, ignore
        }
    }

    public async Task PlayItemSound(string itemName)
    {
        if (!CanUseJS()) return;
        try
        {
            await _js!.InvokeVoidAsync("gameAudio.playItemSound", itemName);
        }
        catch (InvalidOperationException)
        {
            // Ignore
        }
    }

    public async Task PlayBoostSound()
    {
        if (!CanUseJS()) return;
        try
        {
            await _js!.InvokeVoidAsync("gameAudio.playBoostSound");
        }
        catch (InvalidOperationException)
        {
            // Ignore
        }
    }

    public async Task SetVolumes()
    {
        if (!CanUseJS()) return;
        try
        {
            await _js!.InvokeVoidAsync("gameAudio.setVolumes", MasterVolume, MusicVolume, SfxVolume);
        }
        catch (InvalidOperationException)
        {
            // Ignore
        }
    }

    public async Task StopMusic()
    {
        if (!CanUseJS()) return;
        try
        {
            await _js!.InvokeVoidAsync("gameAudio.stopMusic");
        }
        catch (InvalidOperationException)
        {
            // Ignore
        }
    }
}
