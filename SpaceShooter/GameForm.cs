using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SpaceShooter
{
    public partial class GameForm : Form
    {
        private GameEngine _engine;
        private System.Windows.Forms.Timer _gameTimer;
        private HashSet<Keys> _pressedKeys = new HashSet<Keys>();
        private DateTime _lastFrame;

        public GameForm()
        {
            this.Text = "Space Shooter - Jacky The Code Bender";
            this.ClientSize = new Size(800, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;

            _engine = new GameEngine(this.ClientSize.Width, this.ClientSize.Height);
            
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 16; 
            _gameTimer.Tick += GameLoop;
            
            this.KeyDown += (s, e) => {
                _pressedKeys.Add(e.KeyCode);
                HandleGlobalInput(e.KeyCode);
            };
            this.KeyUp += (s, e) => _pressedKeys.Remove(e.KeyCode);
            
            _lastFrame = DateTime.Now;
            _gameTimer.Start();
        }

        private void HandleGlobalInput(Keys key)
        {
            if (key == Keys.Escape)
            {
                _engine.State = GameState.Menu;
            }
            
            if (_engine.State == GameState.Menu)
            {
                if (key == Keys.N) { _engine.InitGame(); _engine.State = GameState.Playing; }
                if (key == Keys.A) _engine.State = GameState.About;
                if (key == Keys.X) Application.Exit();
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            float deltaTime = (float)(DateTime.Now - _lastFrame).TotalSeconds;
            if (deltaTime > 0.1f) deltaTime = 0.016f; // Cap delta to prevent huge jumps
            _lastFrame = DateTime.Now;

            bool left = _pressedKeys.Contains(Keys.Left) || _pressedKeys.Contains(Keys.A);
            bool right = _pressedKeys.Contains(Keys.Right) || _pressedKeys.Contains(Keys.D);
            bool up = _pressedKeys.Contains(Keys.Up) || _pressedKeys.Contains(Keys.W);
            bool down = _pressedKeys.Contains(Keys.Down) || _pressedKeys.Contains(Keys.S);
            bool shoot = _pressedKeys.Contains(Keys.Space) || _pressedKeys.Contains(Keys.K);

            _engine.Update(deltaTime, left, right, up, down, shoot);
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _engine.Draw(e.Graphics);
        }
    }
}
