using Avalonia;
using Avalonia.Controls;
using DiggerNet.ViewModels;

namespace DiggerNet.Views
{
    public partial class GameView : UserControl
    {
        private GameControl? _gameControl;

        public GameView()
        {
            InitializeComponent();
            _gameControl = this.FindControl<GameControl>("GameCanvas");

            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is GameViewModel vm && _gameControl != null)
                {
                    _gameControl.SetGameState(vm.GameState);
                }
            };
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            this.Focus();
        }
    }
}