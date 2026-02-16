using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace CrossWordsNet.Views
{
    public partial class MenuView : UserControl
    {
        public MenuView()
        {
            InitializeComponent();
            LoadImages();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void LoadImages()
        {
            var logoImg = this.FindControl<Image>("LogoImg");
            var backgroundImg = this.FindControl<Image>("BackgroundImg");

            if (logoImg != null)
            {
                logoImg.Source = new Bitmap(AssetLoader.Open(new Uri("avares://CrossWordsNet/Assets/logo.png")));
            }

            if (backgroundImg != null)
            {
                backgroundImg.Source = new Bitmap(AssetLoader.Open(new Uri("avares://CrossWordsNet/Assets/background.png")));
            }
        }
    }
}