using Avalonia;
using Avalonia.ReactiveUI;
using CrossyGame.Utils;
using System;

namespace CrossyGame
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Initialize assets before app starts (requires Avalonia dependency but no UI thread yet, 
            // actually WriteableBitmap requires platform initialized?)
            // Safest to init assets inside App or MainWindow.
            
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}