using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ParticleCombat
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static Vector2 ScreenSize;
        public static Rectangle Viewport { get { return new Rectangle(0,0,(int)ScreenSize.X, (int)ScreenSize.Y);} }

        enum GameState { Menu, Playing, GameOver, Pause }
        GameState CurrentState = GameState.Menu;

        string[] menuOptions = { "NEW GAME", "ABOUT", "EXIT" };
        int currentMenuSelection = 0;

        public static int Score = 0;
        int highscore = 0;

        float enemySpawnTimer = 0;
        float enemySpawnRate = 60; 

        // Background Elements
        struct Star
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public Color Color;
        }
        List<Star> stars = new List<Star>();
        
        struct Planet
        {
            public Vector2 Position;
            public float Speed;
            public float Scale;
            public Color Color;
            public Texture2D Texture;
        }
        List<Planet> planets = new List<Planet>();
        Random rand = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.Title = "Particle Combat - Jacky Edition";
            
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            ScreenSize = new Vector2(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Art.Load(GraphicsDevice);
            SoundSystem.Load(); 
            
            // Initialize Background
            for(int i=0; i<150; i++)
            {
                stars.Add(new Star()
                {
                    Position = new Vector2(rand.Next((int)ScreenSize.X), rand.Next((int)ScreenSize.Y)),
                    Speed = (float)rand.NextDouble() * 0.8f + 0.2f,
                    Scale = (float)rand.NextDouble() * 0.8f + 0.2f,
                    Color = Color.Lerp(Color.White, Color.Yellow, (float)rand.NextDouble()) * ((float)rand.NextDouble() * 0.5f + 0.5f)
                });
            }

            // Add a few planets
            planets.Add(new Planet() { Position = new Vector2(100, 100), Speed = 0.1f, Scale = 2.0f, Color = Color.Red, Texture = Art.Circle });
            planets.Add(new Planet() { Position = new Vector2(800, 600), Speed = 0.05f, Scale = 4.0f, Color = Color.Blue, Texture = Art.Circle });
            planets.Add(new Planet() { Position = new Vector2(900, 200), Speed = 0.15f, Scale = 1.5f, Color = Color.Green, Texture = Art.Circle });
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();

            if (Input.WasKeyPressed(Keys.Escape))
            {
                if (CurrentState == GameState.Playing)
                    CurrentState = GameState.Pause;
                else if (CurrentState == GameState.Pause)
                    CurrentState = GameState.Playing;
                else if (CurrentState == GameState.GameOver)
                    CurrentState = GameState.Menu;
            }

            // Always update background slightly
            UpdateBackground();

            switch (CurrentState)
            {
                case GameState.Menu:
                    UpdateMenu();
                    break;
                case GameState.Playing:
                    UpdateGame();
                    break;
                case GameState.Pause:
                    UpdatePause();
                    break;
                case GameState.GameOver:
                    UpdateGameOver();
                    break;
            }

            ParticleManager.Update();
            base.Update(gameTime);
        }

        private void UpdateBackground()
        {
            // Parallax effect based on player velocity if playing
            Vector2 movement = new Vector2(0, 1); // Default drift down
            if (CurrentState == GameState.Playing && !Player.Instance.IsExpired)
            {
                 movement -= Player.Instance.Velocity * 0.2f;
            }

            for(int i=0; i<stars.Count; i++)
            {
                var s = stars[i];
                s.Position += movement * s.Speed;
                
                // Wrap around
                if (s.Position.Y > ScreenSize.Y) s.Position.Y = 0;
                if (s.Position.Y < 0) s.Position.Y = ScreenSize.Y;
                if (s.Position.X > ScreenSize.X) s.Position.X = 0;
                if (s.Position.X < 0) s.Position.X = ScreenSize.X;
                
                stars[i] = s;
            }

            for(int i=0; i<planets.Count; i++)
            {
                var p = planets[i];
                p.Position += movement * p.Speed * 0.5f;

                 // Wrap around planets
                if (p.Position.Y > ScreenSize.Y + 100) p.Position.Y = -100;
                if (p.Position.Y < -100) p.Position.Y = ScreenSize.Y + 100;
                if (p.Position.X > ScreenSize.X + 100) p.Position.X = -100;
                if (p.Position.X < -100) p.Position.X = ScreenSize.X + 100;

                planets[i] = p;
            }
        }

        private void UpdateMenu()
        {
            if (Input.WasKeyPressed(Keys.Up))
            {
                currentMenuSelection--;
                if (currentMenuSelection < 0) currentMenuSelection = menuOptions.Length - 1;
            }
            if (Input.WasKeyPressed(Keys.Down))
            {
                currentMenuSelection++;
                if (currentMenuSelection >= menuOptions.Length) currentMenuSelection = 0;
            }

            if (Input.WasKeyPressed(Keys.Enter))
            {
                if (currentMenuSelection == 0) StartGame();
                else if (currentMenuSelection == 1) 
                { 
                   // Logic for About
                }
                else if (currentMenuSelection == 2) Exit();
            }
        }

        private void StartGame()
        {
            EntityManager.Clear();
            ParticleManager.Clear();
            Score = 0;
            enemySpawnRate = 60;
            CurrentState = GameState.Playing;
            SoundSystem.Play(SoundSystem.Spawn, 0.5f, 0.0f, 0.0f);
        }

        private void UpdateGame()
        {
            EntityManager.Update();

            if (Player.Instance.IsExpired)
            {
                CurrentState = GameState.GameOver;
                if (Score > highscore) highscore = Score;
            }

            enemySpawnTimer++;
            if (enemySpawnTimer > enemySpawnRate)
            {
                enemySpawnTimer = 0;
                SpawnEnemy();

                if (enemySpawnRate > 20)
                    enemySpawnRate -= 0.1f;
            }
        }

        private void SpawnEnemy()
        {
            Random rand = new Random();
            Vector2 pos = Vector2.Zero;
            
            if (rand.Next(2) == 0)
            {
                pos.X = rand.Next(2) == 0 ? -50 : ScreenSize.X + 50;
                pos.Y = rand.Next((int)ScreenSize.Y);
            }
            else
            {
                pos.X = rand.Next((int)ScreenSize.X);
                pos.Y = rand.Next(2) == 0 ? -50 : ScreenSize.Y + 50;
            }

            // Use the new Alien texture!
            EntityManager.Add(new Enemy(Art.Alien, pos));
        }

        private void UpdatePause()
        {
            if (Input.WasKeyPressed(Keys.Enter))
                CurrentState = GameState.Playing;
            if (Input.WasKeyPressed(Keys.Q))
                CurrentState = GameState.Menu;
        }

        private void UpdateGameOver()
        {
            if (Input.WasKeyPressed(Keys.Enter))
                CurrentState = GameState.Menu;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // Background Layer
            _spriteBatch.Begin();
            foreach(var p in planets)
            {
                 _spriteBatch.Draw(p.Texture, p.Position, null, p.Color, 0f, new Vector2(p.Texture.Width/2, p.Texture.Height/2), p.Scale, SpriteEffects.None, 0f);
            }
            foreach(var s in stars)
            {
                _spriteBatch.Draw(Art.Pixel, s.Position, null, s.Color, 0f, Vector2.Zero, s.Scale * 2, SpriteEffects.None, 0f);
            }
            _spriteBatch.End();

            // Gameplay Layer (Additive for glow)
            _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.Additive);
            if (CurrentState == GameState.Playing || CurrentState == GameState.Pause || CurrentState == GameState.GameOver)
            {
                EntityManager.Draw(_spriteBatch);
                ParticleManager.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            // UI Layer (Normal Blend)
            _spriteBatch.Begin();
            
            if (CurrentState == GameState.Playing)
            {
                TextRenderer.DrawText(_spriteBatch, "SCORE " + Score, new Vector2(20, 20), 3, Color.White);
            }
            else if (CurrentState == GameState.Menu)
            {
                DrawMenu();
            }
            else if (CurrentState == GameState.Pause)
            {
                DrawCenteredText("PAUSED", -50, 5, Color.Yellow);
                DrawCenteredText("PRESS ENTER TO RESUME", 20, 2, Color.White);
                DrawCenteredText("PRESS Q TO QUIT", 50, 2, Color.White);
            }
            else if (CurrentState == GameState.GameOver)
            {
                DrawCenteredText("GAME OVER", -50, 5, Color.Red);
                DrawCenteredText("SCORE " + Score, 10, 3, Color.White);
                DrawCenteredText("HIGHSCORE " + highscore, 40, 3, Color.Gold);
                DrawCenteredText("PRESS ENTER TO MENU", 80, 2, Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawMenu()
        {
            DrawCenteredText("PARTICLE COMBAT", -150, 6, Color.Cyan);
            DrawCenteredText("JACKY THE CODE BENDER", -100, 2, Color.Gray);

            for (int i = 0; i < menuOptions.Length; i++)
            {
                Color c = (i == currentMenuSelection) ? Color.Yellow : Color.White;
                int scale = (i == currentMenuSelection) ? 4 : 3;
                DrawCenteredText(menuOptions[i], i * 50, scale, c);
            }

            if (currentMenuSelection == 1) // About
            {
                 TextRenderer.DrawText(_spriteBatch, "CREATED BY GRAVICODE STUDIOS", new Vector2(50, ScreenSize.Y - 100), 2, Color.DarkGray);
                 TextRenderer.DrawText(_spriteBatch, "CONTROLS WASD AIM MOUSE", new Vector2(50, ScreenSize.Y - 60), 2, Color.DarkGray);
            }
        }

        private void DrawCenteredText(string text, float yOffsetFromCenter, int scale, Color color)
        {
            Vector2 size = TextRenderer.MeasureString(text, scale);
            Vector2 pos = new Vector2((ScreenSize.X - size.X) / 2, (ScreenSize.Y / 2) + yOffsetFromCenter);
            TextRenderer.DrawText(_spriteBatch, text, pos, scale, color);
        }
    }
}