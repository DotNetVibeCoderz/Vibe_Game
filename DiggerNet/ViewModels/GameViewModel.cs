using ReactiveUI;
using DiggerNet.Models;
using System.Windows.Input;

namespace DiggerNet.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private GameState _gameState;
        public GameState GameState
        {
            get => _gameState;
            set => this.RaiseAndSetIfChanged(ref _gameState, value);
        }
        
        public ICommand GoToMenuCommand { get; }

        public GameViewModel(MainWindowViewModel mainViewModel)
        {
            _gameState = new GameState();
            GoToMenuCommand = ReactiveCommand.Create(mainViewModel.GoToMenu);
        }

        public void Start() => GameState.NewGame();
        public void Pause() => GameState.TogglePause();
    }
}