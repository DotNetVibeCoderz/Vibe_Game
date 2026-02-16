using ReactiveUI;
using System.Reactive;
using CrossWordsNet.Services;

namespace CrossWordsNet.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainViewModel;

        public string AppVersion => "Version 1.0.0";
        public string Developer => "Developed by CrossWordsNet Team";
        public string Description => "A Cross-platform Crossword Puzzle Game built with Avalonia UI and .NET.";

        public AboutViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            BackCommand = ReactiveCommand.Create(() => 
            {
                SoundManager.PlayClick();
                _mainViewModel.GoToMenu();
            });
        }

        public ReactiveCommand<Unit, Unit> BackCommand { get; }
    }
}