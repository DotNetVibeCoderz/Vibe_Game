using System.Windows;

namespace DoomNet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// Entry point aplikasi WPF
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Memastikan MainWindow dimuat
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}