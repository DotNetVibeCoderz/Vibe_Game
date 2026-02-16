using ReactiveUI;
using System.Reactive;
using System;
using CrossWordsNet.Services;

namespace CrossWordsNet.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainViewModel;

        public MenuViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            StartNewGameCommand = ReactiveCommand.Create(() => 
            {
                SoundManager.PlayClick();
                _mainViewModel.StartGame();
            });
            
            OptionsCommand = ReactiveCommand.Create(() => 
            {
                SoundManager.PlayClick();
                _mainViewModel.GoToOptions();
            });

            AboutCommand = ReactiveCommand.Create(() => 
            {
                SoundManager.PlayClick();
                _mainViewModel.GoToAbout();
            });

            ExitCommand = ReactiveCommand.Create(() => 
            {
                SoundManager.PlayClick();
                Environment.Exit(0);
            });
        }

        public ReactiveCommand<Unit, Unit> StartNewGameCommand { get; }
        public ReactiveCommand<Unit, Unit> OptionsCommand { get; }
        public ReactiveCommand<Unit, Unit> AboutCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }
    }
}