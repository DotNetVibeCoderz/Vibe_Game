using ReactiveUI;
using System.Windows.Input;
using System;

namespace DiggerNet.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public ICommand StartGameCommand { get; }
        public ICommand OptionsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ExitCommand { get; }

        public MenuViewModel(MainWindowViewModel mainViewModel)
        {
            StartGameCommand = ReactiveCommand.Create(mainViewModel.StartGame);
            OptionsCommand = ReactiveCommand.Create(mainViewModel.GoToOptions);
            AboutCommand = ReactiveCommand.Create(mainViewModel.GoToAbout);
            ExitCommand = ReactiveCommand.Create(() => Environment.Exit(0));
        }
    }
}