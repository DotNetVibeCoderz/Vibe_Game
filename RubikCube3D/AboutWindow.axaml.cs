using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Diagnostics;

namespace RubikCube3D
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void OnLinkClick(object sender, PointerPressedEventArgs e)
        {
            try
            {
                 var url = "https://studios.gravicode.com/products/budax";
                 Process.Start(new ProcessStartInfo
                 {
                     FileName = url,
                     UseShellExecute = true
                 });
            }
            catch { }
        }
    }
}
