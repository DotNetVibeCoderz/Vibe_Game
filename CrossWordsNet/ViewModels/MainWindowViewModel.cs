using ReactiveUI;
using System.Reactive;

namespace CrossWordsNet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentView;

        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        public ReactiveCommand<Unit, Unit> GoToMenuCommand { get; }
        public ReactiveCommand<Unit, Unit> GoToGameCommand { get; }

        public MainWindowViewModel()
        {
            GoToMenuCommand = ReactiveCommand.Create(() => { CurrentView = new MenuViewModel(this); });
            GoToGameCommand = ReactiveCommand.Create(() => { CurrentView = new GameViewModel(this); });
            
            CurrentView = new MenuViewModel(this);
        }

        public void StartGame()
        {
            CurrentView = new GameViewModel(this);
        }

        public void GoToOptions()
        {
            CurrentView = new OptionsViewModel(this);
        }

        public void GoToAbout()
        {
            CurrentView = new AboutViewModel(this);
        }
        
        public void GoToMenu()
        {
            CurrentView = new MenuViewModel(this);
        }
    }
}