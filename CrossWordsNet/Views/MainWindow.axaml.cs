using Avalonia.Controls;
using CrossWordsNet.ViewModels;

namespace CrossWordsNet.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}