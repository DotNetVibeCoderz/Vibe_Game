using Microsoft.JSInterop;

namespace MarioCart3D.Services;

/// <summary>
/// Manages dynamic weather effects for race tracks.
/// </summary>
public class WeatherService
{
    private readonly IJSRuntime _js;

    public WeatherType CurrentWeather { get; set; } = WeatherType.Clear;
    public float Intensity { get; set; } = 0.5f;

    public WeatherService(IJSRuntime js)
    {
        _js = js;
    }

    public void SetRandomWeather()
    {
        var values = Enum.GetValues<WeatherType>();
        CurrentWeather = values[new Random().Next(values.Length)];
    }

    public async Task ApplyWeather(WeatherType weather)
    {
        CurrentWeather = weather;
        var weatherName = weather.ToString().ToLowerInvariant();

        try
        {
            // JS runtime bisa belum siap ketika WebView baru start.
            await _js.InvokeVoidAsync("gameEngine.setWeather", weatherName);
        }
        catch (InvalidOperationException)
        {
            // Ignore: WebView belum siap, efek akan di-apply setelah siap.
        }
    }

    public string GetWeatherColor()
    {
        return CurrentWeather switch
        {
            WeatherType.Clear => "#87CEEB",
            WeatherType.Rain => "#4a5568",
            WeatherType.Snow => "#cbd5e0",
            WeatherType.Fog => "#a0aec0",
            _ => "#87CEEB"
        };
    }
}

public enum WeatherType
{
    Clear,
    Rain,
    Snow,
    Fog
}
