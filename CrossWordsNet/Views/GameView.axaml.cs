using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CrossWordsNet.ViewModels;

namespace CrossWordsNet.Views
{
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            var vm = DataContext as GameViewModel;
            if (vm == null) return;

            switch (e.Key)
            {
                case Key.Left:
                    vm.MoveSelection(0, -1);
                    break;
                case Key.Right:
                    vm.MoveSelection(0, 1);
                    break;
                case Key.Up:
                    vm.MoveSelection(-1, 0);
                    break;
                case Key.Down:
                    vm.MoveSelection(1, 0);
                    break;
                default:
                    // Handle letters
                    var keyStr = e.Key.ToString();
                    if (keyStr.Length == 1 && char.IsLetter(keyStr[0]))
                    {
                         vm.InputLetter(keyStr);
                    }
                    // Handle Number row keys if needed, but Key enum has D0..D9
                    if (e.Key >= Key.A && e.Key <= Key.Z)
                    {
                        // Already handled by ToString usually, but safer:
                        vm.InputLetter(e.Key.ToString()); 
                    }
                    break;
            }
        }
    }
}