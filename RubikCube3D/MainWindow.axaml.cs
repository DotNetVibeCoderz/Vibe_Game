using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using SkiaSharp;
using System;
using System.IO;

namespace RubikCube3D
{
    public partial class MainWindow : Window
    {
        private CubeModel _cube;
        private Renderer _renderer;
        private DispatcherTimer _timer;
        private WriteableBitmap _bitmap;
        
        // Interaction state
        private bool _isDragging;
        private Point _lastMousePos;

        public MainWindow()
        {
            InitializeComponent();
            _cube = new CubeModel(3);
            _renderer = new Renderer();
            
            // Setup loop
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _timer.Tick += (s, e) => UpdateFrame();
            _timer.Start();

            this.Opened += (s, e) => ResizeBitmap();
            this.SizeChanged += (s, e) => ResizeBitmap();
        }

        private void ResizeBitmap()
        {
            var pixelSize = new PixelSize((int)this.Bounds.Width, (int)this.Bounds.Height);
            if (pixelSize.Width <= 0 || pixelSize.Height <= 0) return;

            _bitmap = new WriteableBitmap(pixelSize, new Vector(96, 96), PixelFormat.Bgra8888, AlphaFormat.Premul);
            RenderImage.Source = _bitmap;
        }

        private void UpdateFrame()
        {
            if (_bitmap == null) return;

            using (var buf = _bitmap.Lock())
            {
                var info = new SKImageInfo(buf.Size.Width, buf.Size.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                
                using (var surface = SKSurface.Create(info, buf.Address, buf.RowBytes))
                {
                    if (surface != null)
                    {
                        _renderer.Render(surface.Canvas, _cube, info.Width, info.Height);
                    }
                }
            }
            
            // Force redraw
             RenderImage.InvalidateVisual();
             StatusText.Text = $"Moves: {_cube.MoveHistory.Count} | Size: {_cube.Size}x{_cube.Size}";
        }

        // Input Handling
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _isDragging = true;
                _lastMousePos = e.GetPosition(this);
            }
        }

        private void OnPointerMoved(object sender, PointerEventArgs e)
        {
            if (_isDragging)
            {
                var pos = e.GetPosition(this);
                var delta = pos - _lastMousePos;
                
                _renderer.RotationY += (float)delta.X * 0.5f;
                _renderer.RotationX -= (float)delta.Y * 0.5f;
                
                _lastMousePos = pos;
            }
        }

        private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
        }

        private void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
             if (e.Delta.Y > 0) _renderer.Scale *= 1.1f;
             else _renderer.Scale *= 0.9f;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            string move = "";
            bool shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
            
            switch (e.Key)
            {
                case Key.F: move = "F"; break;
                case Key.B: move = "B"; break;
                case Key.U: move = "U"; break;
                case Key.D: move = "D"; break;
                case Key.L: move = "L"; break;
                case Key.R: move = "R"; break;
            }

            if (!string.IsNullOrEmpty(move))
            {
                if (shift) move += "'";
                _cube.PerformMove(move);
            }
        }

        // Menu Actions
        private void OnNewGame(object sender, RoutedEventArgs e) { _cube.Reset(); }
        private void OnExit(object sender, RoutedEventArgs e) { Close(); }
        private void OnUndo(object sender, RoutedEventArgs e) { _cube.Undo(); }
        private void OnRedo(object sender, RoutedEventArgs e) { _cube.Redo(); }
        private void OnScramble(object sender, RoutedEventArgs e) { _cube.Scramble(); }
        
        private async void OnSolve(object sender, RoutedEventArgs e) 
        {
            // Simple solve: Reverse all moves
            // Animate it?
            while (_cube.MoveHistory.Count > 0)
            {
                _cube.Undo();
                UpdateFrame();
                // To animate, we'd need await Task.Delay, but this blocks UI thread if not careful.
                // Since we are in an async void, we can await.
                await System.Threading.Tasks.Task.Delay(100);
            }
        }

        private void OnDim2(object sender, RoutedEventArgs e) { _cube = new CubeModel(2); }
        private void OnDim3(object sender, RoutedEventArgs e) { _cube = new CubeModel(3); }
        private void OnDim4(object sender, RoutedEventArgs e) { _cube = new CubeModel(4); }
        
        private async void OnAbout(object sender, RoutedEventArgs e)
        {
             var aboutWindow = new AboutWindow();
             await aboutWindow.ShowDialog(this);
        }
    }
}
