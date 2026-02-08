using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace ZombieNet
{
    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Settings,
        About
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Assets
        private Texture2D _grassTexture;
        private Texture2D _roadTexture;
        private Texture2D _towerTexture;
        private Texture2D _zombieTexture;
        private Texture2D _bulletTexture;
        private SimpleFont _font;

        // Game Objects
        public Map Map { get; private set; }
        public List<Tower> Towers { get; private set; }
        public List<Bullet> Bullets { get; private set; }
        public WaveManager WaveManager { get; private set; }

        // State
        public GameState CurrentState { get; private set; }
        public int Gold { get; private set; }
        public int Lives { get; private set; }
        public int Wave => WaveManager != null ? WaveManager.CurrentWave : 1;
        
        private MouseState _prevMouseState;
        private KeyboardState _prevKeyboardState;

        // UI & Camera
        private GameUI _ui;
        private Camera _camera;
        private int _selectedTowerCost = 50; // Default tower cost

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1024; // Wider for UI
            _graphics.PreferredBackBufferHeight = 768; 
        }

        protected override void Initialize()
        {
            CurrentState = GameState.Menu;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load assets
            _grassTexture = TextureLoader.Load(GraphicsDevice, "grass.png");
            _roadTexture = TextureLoader.Load(GraphicsDevice, "road.png");
            _towerTexture = TextureLoader.Load(GraphicsDevice, "tower.png");
            _zombieTexture = TextureLoader.Load(GraphicsDevice, "zombie.png");
            _bulletTexture = TextureLoader.Load(GraphicsDevice, "bullet.png");

            // Load Font
            _font = new SimpleFont(GraphicsDevice);

            // Initialize UI
            _ui = new GameUI(GraphicsDevice, _font);
            _ui.AssignActions(this);

            // Initialize Sound
            SoundManager.LoadContent(Content);

            // Start Game Logic (but remain in Menu)
            ResetGame();
        }

        public void StartNewGame()
        {
            ResetGame();
            CurrentState = GameState.Playing;
        }

        private void ResetGame()
        {
            Towers = new List<Tower>();
            Bullets = new List<Bullet>();
            
            // Map 20x20
            Map = new Map(20, 20); 

            WaveManager = new WaveManager(Map.Path, _zombieTexture);
            
            Gold = 300;
            Lives = 20;
            
            // Initialize Camera
            _camera = new Camera(
                _graphics.PreferredBackBufferWidth - 200, // Viewport width (minus sidebar)
                _graphics.PreferredBackBufferHeight,      // Viewport height
                Map.Width * Tile.Size,                    // World width
                Map.Height * Tile.Size                    // World height
            );
        }

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
        }

        public void SelectTower(int towerType)
        {
            // Simple logic for now: 1 = Basic ($50), 2 = Advanced ($100)
            if (towerType == 1) _selectedTowerCost = 50;
            else if (towerType == 2) _selectedTowerCost = 100;
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            // Toggle Pause
            if (keyState.IsKeyDown(Keys.Escape) && _prevKeyboardState.IsKeyUp(Keys.Escape))
            {
                if (CurrentState == GameState.Playing) CurrentState = GameState.Paused;
                else if (CurrentState == GameState.Paused) CurrentState = GameState.Playing;
                else if (CurrentState == GameState.Settings || CurrentState == GameState.About) CurrentState = GameState.Menu;
            }

            // UI Update
            _ui.Update(gameTime, mouseState, _prevMouseState, CurrentState, this);

            if (CurrentState == GameState.Playing)
            {
                UpdateGame(gameTime, mouseState, keyState);
            }

            _prevKeyboardState = keyState;
            _prevMouseState = mouseState;
            base.Update(gameTime);
        }

        private void UpdateGame(GameTime gameTime, MouseState mouseState, KeyboardState keyState)
        {
            // Camera Movement (WASD or Arrow Keys)
            Vector2 camMove = Vector2.Zero;
            float camSpeed = 500f * (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            if (keyState.IsKeyDown(Keys.W) || keyState.IsKeyDown(Keys.Up)) camMove.Y -= camSpeed;
            if (keyState.IsKeyDown(Keys.S) || keyState.IsKeyDown(Keys.Down)) camMove.Y += camSpeed;
            if (keyState.IsKeyDown(Keys.A) || keyState.IsKeyDown(Keys.Left)) camMove.X -= camSpeed;
            if (keyState.IsKeyDown(Keys.D) || keyState.IsKeyDown(Keys.Right)) camMove.X += camSpeed;
            
            _camera.Move(camMove);

            // Game Logic
            WaveManager.Update(gameTime);
            
            // Bullet Logic
            for (int i = Bullets.Count - 1; i >= 0; i--)
            {
                Bullets[i].Update(gameTime);
                if (!Bullets[i].IsActive) Bullets.RemoveAt(i);
            }

            // Check collisions/damage
            var enemies = WaveManager.Enemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i].ReachedEnd)
                {
                    Lives--;
                    enemies.RemoveAt(i);
                    if (Lives <= 0) CurrentState = GameState.GameOver;
                }
                else if (enemies[i].IsDead)
                {
                    Gold += enemies[i].Bounty;
                    enemies.RemoveAt(i);
                }
            }

            // Tower Logic
            foreach (var tower in Towers)
            {
                tower.Update(gameTime, enemies, Bullets);
            }

            // Build Tower
            // Only if mouse is NOT over UI sidebar (Sidebar is > Width - 200)
            if (mouseState.X < _graphics.PreferredBackBufferWidth - 200)
            {
                if (mouseState.LeftButton == ButtonState.Pressed && _prevMouseState.LeftButton == ButtonState.Released)
                {
                    // Convert screen pos to world pos
                    Vector2 worldPos = _camera.ScreenToWorld(new Vector2(mouseState.X, mouseState.Y));
                    int gridX = (int)(worldPos.X / Tile.Size);
                    int gridY = (int)(worldPos.Y / Tile.Size);

                    if (gridX >= 0 && gridX < Map.Width && gridY >= 0 && gridY < Map.Height)
                    {
                        Tile tile = Map.Tiles[gridX, gridY];
                        if (tile.IsBuildable && Gold >= _selectedTowerCost)
                        {
                            bool occupied = Towers.Any(t => Vector2.Distance(t.Position, tile.Position + new Vector2(32, 32)) < 10);
                            if (!occupied)
                            {
                                Vector2 pos = tile.Position + new Vector2(32, 32);
                                float range = _selectedTowerCost == 50 ? 150 : 250;
                                float fireRate = _selectedTowerCost == 50 ? 1.0f : 0.5f;
                                int damage = _selectedTowerCost == 50 ? 20 : 40;
                                
                                Towers.Add(new Tower(_towerTexture, _bulletTexture, pos, range, damage, fireRate));
                                Gold -= _selectedTowerCost;
                            }
                        }
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw Game World with Camera
            if (CurrentState == GameState.Playing || CurrentState == GameState.Paused || CurrentState == GameState.GameOver)
            {
                _spriteBatch.Begin(transformMatrix: _camera.Transform);
                
                // Map
                Map.Draw(_spriteBatch, _grassTexture, _roadTexture);
                
                // Enemies
                WaveManager.Draw(_spriteBatch);

                // Towers
                foreach (var tower in Towers) tower.Draw(_spriteBatch);

                // Bullets
                foreach (var bullet in Bullets) bullet.Draw(_spriteBatch);

                _spriteBatch.End();
            }

            // Draw UI (No Camera)
            _spriteBatch.Begin();
            _ui.Draw(_spriteBatch, CurrentState, this);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
