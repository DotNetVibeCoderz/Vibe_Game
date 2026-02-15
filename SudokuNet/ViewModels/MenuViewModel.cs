using ReactiveUI;
using System;
using System.Reactive;

namespace SudokuNet.ViewModels
{
    public class MenuViewModel : ViewModelBase
    {
        public ReactiveCommand<string, Unit> StartGameCommand { get; }

        public MenuViewModel(Action<string> startGameAction)
        {
            StartGameCommand = ReactiveCommand.Create<string>(difficulty =>
            {
                startGameAction(difficulty);
            });
        }
    }
}