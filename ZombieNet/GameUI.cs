using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace ZombieNet
{
    public class GameUI
    {
        private Texture2D _whiteTexture;
        private SimpleFont _font;
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        // Menus
        private List<Button> _mainMenuButtons;
        private List<Button> _pauseMenuButtons;
        private List<Button> _gameOverButtons;
        
        // In-Game UI
        private Rectangle _sidebarRect;
        private List<Button> _towerButtons;
        private Rectangle _minimapRect;

        public GameUI(GraphicsDevice graphicsDevice, SimpleFont font)
        {
            _graphicsDevice = graphicsDevice;
            _font = font;
            
            _whiteTexture = new Texture2D(graphicsDevice, 1, 1);
            _whiteTexture.SetData(new[] { Color.White });

            InitializeMenus();
        }

        private void InitializeMenus()
        {
            int screenWidth = _graphicsDevice.Viewport.Width; // 1024
            int screenHeight = _graphicsDevice.Viewport.Height; // 768
            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;

            // Main Menu
            _mainMenuButtons = new List<Button>
            {
                new Button(new Rectangle(centerX - 100, centerY - 80, 200, 50), "NEW GAME"),
                new Button(new Rectangle(centerX - 100, centerY - 20, 200, 50), "SETTINGS"),
                new Button(new Rectangle(centerX - 100, centerY + 40, 200, 50), "ABOUT"),
                new Button(new Rectangle(centerX - 100, centerY + 100, 200, 50), "EXIT")
            };

            // Pause Menu
            _pauseMenuButtons = new List<Button>
            {
                new Button(new Rectangle(centerX - 100, centerY - 50, 200, 50), "RESUME"),
                new Button(new Rectangle(centerX - 100, centerY + 10, 200, 50), "MAIN MENU"),
                new Button(new Rectangle(centerX - 100, centerY + 70, 200, 50), "EXIT")
            };

            // Game Over Menu
            _gameOverButtons = new List<Button>
            {
                new Button(new Rectangle(centerX - 100, centerY - 20, 200, 50), "RETRY"),
                new Button(new Rectangle(centerX - 100, centerY + 40, 200, 50), "MAIN MENU")
            };

            // Sidebar (Right 200px)
            _sidebarRect = new Rectangle(screenWidth - 200, 0, 200, screenHeight);
            
            // Minimap (Top of sidebar)
            _minimapRect = new Rectangle(screenWidth - 190, 10, 180, 180);

            // Tower Buttons (Below minimap)
            _towerButtons = new List<Button>
            {
                new Button(new Rectangle(screenWidth - 180, 210, 160, 40), "TOWER 1 ($50)"),
                new Button(new Rectangle(screenWidth - 180, 260, 160, 40), "TOWER 2 ($100)")
            };
        }

        public void Update(GameTime gameTime, MouseState mouseState, MouseState prevMouseState, GameState state, Game1 game)
        {
            switch (state)
            {
                case GameState.Menu:
                    HandleMenuInput(_mainMenuButtons, mouseState, prevMouseState, game);
                    break;
                case GameState.Paused:
                    HandleMenuInput(_pauseMenuButtons, mouseState, prevMouseState, game);
                    break;
                case GameState.GameOver:
                    HandleMenuInput(_gameOverButtons, mouseState, prevMouseState, game);
                    break;
                case GameState.Playing:
                    HandleInGameInput(mouseState, prevMouseState, game);
                    break;
            }
        }

        private void HandleMenuInput(List<Button> buttons, MouseState mouseState, MouseState prevMouseState, Game1 game)
        {
            foreach (var btn in buttons)
            {
                btn.Update(mouseState, prevMouseState);
                if (btn.IsHovered && mouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
                {
                    // Logic handled in Draw or via callback, but here we invoke Action.
                    // Oh, Button already invokes OnClick. We just need to assign actions.
                }
            }
        }

        private void HandleInGameInput(MouseState mouseState, MouseState prevMouseState, Game1 game)
        {
            foreach (var btn in _towerButtons)
            {
                btn.Update(mouseState, prevMouseState);
            }
            
            // Minimap scrolling logic could go here
        }

        public void Draw(SpriteBatch spriteBatch, GameState state, Game1 game)
        {
            if (state == GameState.Playing)
            {
                DrawHUD(spriteBatch, game);
            }
            else
            {
                // Draw semi-transparent background for menus
                spriteBatch.Draw(_whiteTexture, new Rectangle(0, 0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height), Color.Black * 0.7f);

                if (state == GameState.Menu) DrawMenu(spriteBatch, _mainMenuButtons, "ZOMBIE NET");
                else if (state == GameState.Paused) DrawMenu(spriteBatch, _pauseMenuButtons, "PAUSED");
                else if (state == GameState.GameOver) DrawMenu(spriteBatch, _gameOverButtons, "GAME OVER");
                else if (state == GameState.About) DrawAbout(spriteBatch);
                else if (state == GameState.Settings) DrawSettings(spriteBatch);
            }
        }

        private void DrawMenu(SpriteBatch spriteBatch, List<Button> buttons, string title)
        {
            // Draw Title
            Vector2 titleSize = _font.MeasureString(title, 4.0f);
            _font.DrawString(spriteBatch, title, 
                new Vector2((_graphicsDevice.Viewport.Width - titleSize.X) / 2, 100), 
                Color.Red, 4.0f);

            foreach (var btn in buttons)
            {
                btn.Draw(spriteBatch, _whiteTexture, _font);
            }
        }

        private void DrawAbout(SpriteBatch spriteBatch)
        {
            string text = "ZOMBIE NET\n\nCREATED BY\nGRAVICODE STUDIOS\n\nJACKY THE CODE BENDER\n\n2024";
            Vector2 size = _font.MeasureString(text, 2.0f);
             _font.DrawString(spriteBatch, text, 
                new Vector2((_graphicsDevice.Viewport.Width - size.X) / 2, 200), 
                Color.White, 2.0f);
             
             // Back button logic needed here or just press ESC
             string hint = "PRESS ESC TO RETURN";
             Vector2 hintSize = _font.MeasureString(hint, 2.0f);
             _font.DrawString(spriteBatch, hint, 
                new Vector2((_graphicsDevice.Viewport.Width - hintSize.X) / 2, 500), 
                Color.Gray, 2.0f);
        }

        private void DrawSettings(SpriteBatch spriteBatch)
        {
             string text = "SETTINGS\n\nSOUND: ON\n\n(NOT IMPLEMENTED YET)";
             Vector2 size = _font.MeasureString(text, 2.0f);
             _font.DrawString(spriteBatch, text, 
                new Vector2((_graphicsDevice.Viewport.Width - size.X) / 2, 200), 
                Color.White, 2.0f);
             
             string hint = "PRESS ESC TO RETURN";
             Vector2 hintSize = _font.MeasureString(hint, 2.0f);
             _font.DrawString(spriteBatch, hint, 
                new Vector2((_graphicsDevice.Viewport.Width - hintSize.X) / 2, 500), 
                Color.Gray, 2.0f);
        }

        private void DrawHUD(SpriteBatch spriteBatch, Game1 game)
        {
            // Sidebar Background
            spriteBatch.Draw(_whiteTexture, _sidebarRect, Color.DarkSlateGray);
            
            // Minimap
            spriteBatch.Draw(_whiteTexture, _minimapRect, Color.Black);
            // Draw mini dots for map
            // We need access to map data. Assuming game exposes Map.
            // Scale factors
            float scaleX = (float)_minimapRect.Width / (game.Map.Width * Tile.Size);
            float scaleY = (float)_minimapRect.Height / (game.Map.Height * Tile.Size);
            
            // Draw Enemies on Minimap
            foreach(var enemy in game.WaveManager.Enemies)
            {
                 Vector2 miniPos = new Vector2(_minimapRect.X + enemy.Position.X * scaleX, _minimapRect.Y + enemy.Position.Y * scaleY);
                 spriteBatch.Draw(_whiteTexture, new Rectangle((int)miniPos.X, (int)miniPos.Y, 4, 4), Color.Red);
            }
            
            // Draw Towers on Minimap
            foreach(var tower in game.Towers)
            {
                 Vector2 miniPos = new Vector2(_minimapRect.X + tower.Position.X * scaleX, _minimapRect.Y + tower.Position.Y * scaleY);
                 spriteBatch.Draw(_whiteTexture, new Rectangle((int)miniPos.X, (int)miniPos.Y, 4, 4), Color.Blue);
            }

            // Stats
            int startY = 320;
            _font.DrawString(spriteBatch, $"WAVE: {game.Wave}", new Vector2(_sidebarRect.X + 10, startY), Color.White);
            _font.DrawString(spriteBatch, $"GOLD: {game.Gold}", new Vector2(_sidebarRect.X + 10, startY + 30), Color.Yellow);
            _font.DrawString(spriteBatch, $"LIVES: {game.Lives}", new Vector2(_sidebarRect.X + 10, startY + 60), Color.Red);

            // Tower Buttons
            foreach (var btn in _towerButtons)
            {
                btn.Draw(spriteBatch, _whiteTexture, _font);
            }
        }
        
        public void AssignActions(Game1 game)
        {
             _mainMenuButtons[0].OnClick = () => game.StartNewGame();
             _mainMenuButtons[1].OnClick = () => game.ChangeState(GameState.Settings);
             _mainMenuButtons[2].OnClick = () => game.ChangeState(GameState.About);
             _mainMenuButtons[3].OnClick = () => game.Exit();

             _pauseMenuButtons[0].OnClick = () => game.ChangeState(GameState.Playing);
             _pauseMenuButtons[1].OnClick = () => game.ChangeState(GameState.Menu);
             _pauseMenuButtons[2].OnClick = () => game.Exit();

             _gameOverButtons[0].OnClick = () => game.StartNewGame();
             _gameOverButtons[1].OnClick = () => game.ChangeState(GameState.Menu);
             
             _towerButtons[0].OnClick = () => game.SelectTower(1);
             _towerButtons[1].OnClick = () => game.SelectTower(2);
        }
    }
}
