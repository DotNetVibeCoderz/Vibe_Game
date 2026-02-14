using ReactiveUI;
using System.Windows.Input;
using DiggerNet.Audio;

namespace DiggerNet.ViewModels
{
    public class OptionsViewModel : ViewModelBase
    {
        private bool _isSoundEnabled;
        public bool IsSoundEnabled
        {
            get => _isSoundEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _isSoundEnabled, value);
                SoundManager.IsSoundEnabled = value;
            }
        }

        public ICommand BackCommand { get; }

        public OptionsViewModel(MainWindowViewModel mainViewModel)
        {
            IsSoundEnabled = SoundManager.IsSoundEnabled;
            BackCommand = ReactiveCommand.Create(mainViewModel.GoToMenu);
        }
    }
}