using Avalonia.Controls;
using Avalonia.Input;
using DiggerNet.ViewModels;
using DiggerNet.Models;
using System;

namespace DiggerNet.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContextChanged += (s, e) =>
            {
                if (DataContext is MainWindowViewModel vm)
                {
                    _viewModel = vm;
                }
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_viewModel == null) return;

            // Global Shortcuts
            if (e.Key == Key.Escape)
            {
                if (_viewModel.CurrentViewModel is GameViewModel)
                {
                    _viewModel.GoToMenu();
                    e.Handled = true;
                    return;
                }
            }

            // Game Controls
            if (_viewModel.CurrentViewModel is GameViewModel gameVM)
            {
                var gs = gameVM.GameState;
                if (gs == null) return;

                switch (e.Key)
                {
                    case Key.Up: gs.MovePlayer(Direction.Up); break;
                    case Key.Down: gs.MovePlayer(Direction.Down); break;
                    case Key.Left: gs.MovePlayer(Direction.Left); break;
                    case Key.Right: gs.MovePlayer(Direction.Right); break;
                    case Key.F1: gs.Fire(); break;
                    case Key.Space: 
                        if (gs.IsGameOver) gs.NewGame();
                        else gs.TogglePause(); 
                        break;
                    case Key.Add:
                    case Key.OemPlus:
                        gs.GameSpeedMultiplier += 0.1;
                        break;
                    case Key.Subtract:
                    case Key.OemMinus:
                        gs.GameSpeedMultiplier -= 0.1;
                        if (gs.GameSpeedMultiplier < 0.1) gs.GameSpeedMultiplier = 0.1;
                        break;
                }
                
                // If in game, handle arrow keys so they don't do weird focus navigation if somehow focused on something else
                if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
                {
                    e.Handled = true;
                }
            }
            
            base.OnKeyDown(e);
        }
    }
}