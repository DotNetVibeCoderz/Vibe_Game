using Breakout.Core;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace Breakout;

/// <summary>
/// Form utama game Breakout. Menggunakan SkiaSharp untuk rendering.
/// </summary>
public partial class Form1 : Form
{
    private readonly GameEngine _engine;
    private readonly SKControl _skiaControl;
    private readonly System.Windows.Forms.Timer _gameTimer;
    private bool _isRunning = true;

    public Form1()
    {
        // Set form properties
        this.Text = "Breakout - Neon Edition";
        this.ClientSize = new Size(800, 600);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.Black;
        this.KeyPreview = true;
        this.DoubleBuffered = true;

        // Setup SkiaSharp control
        _skiaControl = new SKControl
        {
            Dock = DockStyle.Fill,
            Location = new Point(0, 0),
            Size = new Size(800, 600)
        };
        _skiaControl.PaintSurface += OnPaintSurface;
        this.Controls.Add(_skiaControl);

        // Inisialisasi game engine
        _engine = new GameEngine();

        // Setup keyboard events
        this.KeyDown += OnKeyDown;
        this.KeyUp += OnKeyUp;
        _skiaControl.KeyDown += OnKeyDown;
        _skiaControl.KeyUp += OnKeyUp;

        // Setup mouse events
        _skiaControl.MouseMove += OnMouseMove;
        _skiaControl.MouseDown += OnMouseDown;

        // Game loop timer (60 FPS)
        _gameTimer = new System.Windows.Forms.Timer
        {
            Interval = 16 // ~60 FPS
        };
        _gameTimer.Tick += GameLoop;
        _gameTimer.Start();

        // Handle form closing
        this.FormClosing += OnFormClosing;
    }

    /// <summary>
    /// Game loop utama - dipanggil setiap 16ms.
    /// </summary>
    private void GameLoop(object? sender, EventArgs e)
    {
        if (!_isRunning) return;

        try
        {
            // Update game logic
            _engine.Update();

            // Trigger render
            _skiaControl.Invalidate();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Game loop error: {ex.Message}");
        }
    }

    /// <summary>
    /// Render menggunakan SkiaSharp.
    /// </summary>
    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Black);

        // Render game
        _engine.Render(canvas);
    }

    /// <summary>
    /// Handler untuk keyboard key down.
    /// </summary>
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        _engine.Input.KeyDown(e.KeyCode);

        // Escape untuk pause/menu
        if (e.KeyCode == Keys.Escape)
        {
            if (_engine.State == GameState.Playing)
                _engine.SetState(GameState.Paused);
            else if (_engine.State == GameState.Paused)
                _engine.SetState(GameState.Playing);
            else if (_engine.State == GameState.Settings || _engine.State == GameState.About)
                _engine.SetState(GameState.Menu);
        }

        // Pause toggle
        if (e.KeyCode == Keys.P && _engine.State == GameState.Playing)
            _engine.SetState(GameState.Paused);
        else if (e.KeyCode == Keys.P && _engine.State == GameState.Paused)
            _engine.SetState(GameState.Playing);

        e.Handled = true;
    }

    /// <summary>
    /// Handler untuk keyboard key up.
    /// </summary>
    private void OnKeyUp(object? sender, KeyEventArgs e)
    {
        _engine.Input.KeyUp(e.KeyCode);
        e.Handled = true;
    }

    /// <summary>
    /// Handler untuk mouse move.
    /// </summary>
    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        _engine.Input.SetMousePosition(e.X, e.Y);
    }

    /// <summary>
    /// Handler untuk mouse down.
    /// </summary>
    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            _engine.Input.SetMouseDown(true);
    }

    /// <summary>
    /// Handler untuk form closing.
    /// </summary>
    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        _isRunning = false;
        _gameTimer.Stop();
        _engine.Settings.Save();
    }
}
