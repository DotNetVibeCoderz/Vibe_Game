using ReactiveUI;
using System.Reactive;

namespace SudokuNet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _content = null!;
        public ViewModelBase Content
        {
            get => _content;
            private set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            ShowMenu();
        }

        public void ShowMenu()
        {
            Content = new MenuViewModel(ShowGame);
        }

        public void ShowGame(string difficulty)
        {
            Content = new GameViewModel(difficulty, ShowMenu);
        }
    }
}