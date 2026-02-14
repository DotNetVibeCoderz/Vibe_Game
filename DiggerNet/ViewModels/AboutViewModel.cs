using ReactiveUI;
using System.Windows.Input;

namespace DiggerNet.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public string Title { get; } = "DiggerNet";
        public string Version { get; } = "1.0.0";
        public string Author { get; } = "Jacky the Code Bender";
        
        public ICommand BackCommand { get; }

        public AboutViewModel(MainWindowViewModel mainViewModel)
        {
            BackCommand = ReactiveCommand.Create(mainViewModel.GoToMenu);
        }
    }
}