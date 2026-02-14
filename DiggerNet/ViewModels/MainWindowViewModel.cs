using ReactiveUI;
using System.Windows.Input;

namespace DiggerNet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public MenuViewModel MenuVM { get; }
        public GameViewModel GameVM { get; }
        public AboutViewModel AboutVM { get; }
        public OptionsViewModel OptionsVM { get; }

        public MainWindowViewModel()
        {
            MenuVM = new MenuViewModel(this);
            GameVM = new GameViewModel(this);
            AboutVM = new AboutViewModel(this);
            OptionsVM = new OptionsViewModel(this);
            
            _currentViewModel = MenuVM;
        }

        public void StartGame()
        {
            GameVM.Start();
            CurrentViewModel = GameVM;
        }

        public void GoToMenu()
        {
            GameVM.Pause(); // If coming from game
            CurrentViewModel = MenuVM;
        }

        public void GoToOptions()
        {
            CurrentViewModel = OptionsVM;
        }

        public void GoToAbout()
        {
            CurrentViewModel = AboutVM;
        }
    }
}