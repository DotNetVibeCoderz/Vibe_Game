using MarioCart3D.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace MarioCart3D;

/// <summary>
/// Main WPF window hosting the Blazor WebView.
/// Configures dependency injection for game services and enables debugging.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "--auto-open-devtools-for-tabs");
#endif

        var services = new ServiceCollection();
        services.AddWpfBlazorWebView();
        services.AddSingleton<GameDataService>();
        services.AddSingleton<GameStateService>();
        services.AddSingleton<RaceManager>();
        services.AddSingleton<AiRacerService>();
        services.AddSingleton<ItemManager>();
        services.AddSingleton<ProgressionService>();

        // WeatherService butuh IJSRuntime, jadi harus scoped agar sesuai lifecycle Blazor.
        services.AddScoped<WeatherService>();

        // AudioService juga mengandalkan JS runtime -> scoped.
        services.AddScoped<AudioService>();

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif

        Resources.Add("services", services.BuildServiceProvider());

        InitializeComponent();

#if DEBUG
        Loaded += MainWindow_Loaded;
        KeyDown += MainWindow_KeyDown;
#endif
    }

#if DEBUG
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (BlazorWebView?.WebView != null)
        {
            BlazorWebView.WebView.CoreWebView2InitializationCompleted += OnCoreWebView2InitializationCompleted;
        }
    }

    private void OnCoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
    {
        if (e.IsSuccess && BlazorWebView?.WebView?.CoreWebView2 != null)
        {
            BlazorWebView.WebView.CoreWebView2.OpenDevToolsWindow();
        }
    }

    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F12)
        {
            try
            {
                BlazorWebView.WebView.CoreWebView2?.OpenDevToolsWindow();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DevTools error: {ex.Message}");
            }
        }
    }
#endif
}
