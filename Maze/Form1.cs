using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.IO;

namespace Maze
{
    public class Form1 : Form
    {
        private GameEngine _engine;
        private DirectBitmap _buffer;
        private System.Windows.Forms.Timer _timer;
        private bool _up, _down, _left, _right;
        private bool _isPaused = false;
        private int _lastPoints = 0;
        private SoundPlayer _pickupSound;
        private SoundPlayer _winSound;

        private string _message = "";
        private int _messageTimer = 0;

        // Menu items
        private int _menuSelection = 0;
        private string[] _menuItems = { "Continue", "New Game", "About", "Exit" };

        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(1024, 768); // Increased resolution for better visibility
            this.Text = "Maze 3D - Jacky's Doom Clone";
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Initialize Game
            _engine = new GameEngine();
            _engine.LoadLevel(1); 
            _buffer = new DirectBitmap(Width, Height);

            // Sounds (Mock generation if missing)
            if (!File.Exists("pickup.wav")) AssetGen.CreateSoundFile("pickup.wav", 800, 100);
            if (!File.Exists("win.wav")) AssetGen.CreateSoundFile("win.wav", 1200, 300);
            try { _pickupSound = new SoundPlayer("pickup.wav"); } catch {}
            try { _winSound = new SoundPlayer("win.wav"); } catch {}

            // Timer Loop
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 16;
            _timer.Tick += GameLoop;
            _timer.Start();

            // Events
            this.KeyDown += OnKeyDown;
            this.KeyUp += OnKeyUp;
            this.Paint += OnPaint;
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (_isPaused) 
            {
                this.Invalidate();
                return;
            }

            // Message timer
            if (_messageTimer > 0) _messageTimer--;
            else _message = "";

            // Update Physics
            _engine.Update(_up, _down, _left, _right, 0.016);

            // Check Score Change for Sound
            if (_engine.Points > _lastPoints)
            {
                _lastPoints = _engine.Points;
                try { _pickupSound.Play(); } catch {}
                ShowMessage("+10 POINTS!");
            }

            // Check Win
            if (_engine.LevelFinished)
            {
                try { _winSound.Play(); } catch {}
                _engine.LoadLevel(_engine.Level + 1);
                ShowMessage($"LEVEL {_engine.Level} START!");
            }

            // Render 3D
            _engine.Render(_buffer, Width, Height);

            // Request Redraw
            this.Invalidate();
        }

        private void ShowMessage(string msg)
        {
            _message = msg;
            _messageTimer = 100; // ~1.5 seconds
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            // Draw 3D Buffer
            if (_buffer != null && _buffer.Bitmap != null)
                e.Graphics.DrawImage(_buffer.Bitmap, 0, 0);

            // Calculate Distance to Finish
            double distToFinish = Math.Sqrt(Math.Pow(_engine.PosX - _engine.FinishPoint.X, 2) + Math.Pow(_engine.PosY - _engine.FinishPoint.Y, 2));

            // Draw HUD
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Background box for HUD to make text readable
            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, 0, 0, 0)), 5, 5, 250, 100);

            e.Graphics.DrawString($"LEVEL: {_engine.Level}", new Font("Consolas", 14, FontStyle.Bold), Brushes.Yellow, 10, 10);
            e.Graphics.DrawString($"POINTS: {_engine.Points}", new Font("Consolas", 14, FontStyle.Bold), Brushes.Yellow, 10, 35);
            e.Graphics.DrawString($"TREASURES: {_engine.Sprites.Count}", new Font("Consolas", 12), Brushes.White, 10, 60);
            
            // Re-added Distance Info
            e.Graphics.DrawString($"DIST. TO EXIT: {distToFinish:F1}m", new Font("Consolas", 12), Brushes.Lime, 10, 80);

            // Message Overlay (Center Screen)
            if (!string.IsNullOrEmpty(_message))
            {
                Font msgFont = new Font("Arial", 36, FontStyle.Bold);
                SizeF size = e.Graphics.MeasureString(_message, msgFont);
                float msgX = (Width - size.Width) / 2;
                float msgY = Height / 3;
                
                // Shadow
                e.Graphics.DrawString(_message, msgFont, Brushes.Black, msgX + 2, msgY + 2);
                // Text
                e.Graphics.DrawString(_message, msgFont, Brushes.Cyan, msgX, msgY);
            }

            // Draw Menu if Paused
            if (_isPaused)
            {
                using (Brush overlay = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                {
                    e.Graphics.FillRectangle(overlay, 0, 0, Width, Height);
                }

                int menuY = Height / 2 - 50;
                for (int i = 0; i < _menuItems.Length; i++)
                {
                    Brush brush = (i == _menuSelection) ? Brushes.Red : Brushes.White;
                    string text = (i == _menuSelection) ? $"> {_menuItems[i]} <" : _menuItems[i];
                    Font menuFont = new Font("Arial", 24, FontStyle.Bold);
                    SizeF size = e.Graphics.MeasureString(text, menuFont);
                    e.Graphics.DrawString(text, menuFont, brush, (Width - size.Width) / 2, menuY + (i * 50));
                }
                
                string helpText = "Use ARROW KEYS/WASD to move, ENTER to select, ESC to Pause";
                SizeF helpSize = e.Graphics.MeasureString(helpText, new Font("Arial", 12));
                e.Graphics.DrawString(helpText, new Font("Arial", 12), Brushes.Gray, (Width - helpSize.Width)/2, Height - 50);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _isPaused = !_isPaused;
                _menuSelection = 0;
                this.Invalidate();
            }

            if (_isPaused)
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W) 
                {
                    _menuSelection--;
                    if (_menuSelection < 0) _menuSelection = _menuItems.Length - 1;
                    this.Invalidate();
                }
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S) 
                {
                    _menuSelection++;
                    if (_menuSelection >= _menuItems.Length) _menuSelection = 0;
                    this.Invalidate();
                }
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space) HandleMenu();
                return;
            }

            switch (e.KeyCode)
            {
                case Keys.W: case Keys.Up: _up = true; break;
                case Keys.S: case Keys.Down: _down = true; break;
                case Keys.A: case Keys.Left: _left = true; break;
                case Keys.D: case Keys.Right: _right = true; break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: case Keys.Up: _up = false; break;
                case Keys.S: case Keys.Down: _down = false; break;
                case Keys.A: case Keys.Left: _left = false; break;
                case Keys.D: case Keys.Right: _right = false; break;
            }
        }

        private void HandleMenu()
        {
            switch (_menuSelection)
            {
                case 0: // Continue
                    _isPaused = false;
                    break;
                case 1: // New Game
                    _engine.LoadLevel(1);
                    _isPaused = false;
                    break;
                case 2: // About
                    MessageBox.Show("Maze 3D by Jacky.\nGravicode Studios.\n\nFind the exit door!\nCollect treasures for points.", "About");
                    break;
                case 3: // Exit
                    Application.Exit();
                    break;
            }
            this.Invalidate();
        }

        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            if (_buffer != null) _buffer.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }
    }
}