using ReactiveUI;
using System.Reactive;
using CrossWordsNet.Services;

namespace CrossWordsNet.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainViewModel;
        
        // Example option
        private bool _soundEnabled = true;
        public bool SoundEnabled
        {
            get => _soundEnabled;
            set => this.RaiseAndSetIfChanged(ref _soundEnabled, value);
        }

        public OptionsViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            BackCommand = ReactiveCommand.Create(() => 
            {
                SoundManager.PlayClick();
                _mainViewModel.GoToMenu();
            });
            ToggleSoundCommand = ReactiveCommand.Create(() => 
            {
                SoundManager.PlayClick();
                // Logic to toggle sound global setting could go here
            });
        }

        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        public ReactiveCommand<Unit, Unit> ToggleSoundCommand { get; }
    }
}