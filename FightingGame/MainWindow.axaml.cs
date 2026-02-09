using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;

namespace FightingGame
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _gameTimer;
        private bool _isGameRunning = false;
        private int _timeLeft = 99;
        private DateTime _lastTimeTick;

        // Sound
        private SoundManager _soundManager;
        private double _hitSoundCooldown = 0;

        // Characters
        private Character? _player;
        private Character? _enemy;

        // Input
        private bool _leftPressed, _rightPressed, _jumpPressed, _punchPressed, _kickPressed, _blockPressed;

        // Animations Cache
        private Dictionary<string, Dictionary<ActionState, List<Bitmap>>> _loadedAnimations = new();

        public MainWindow()
        {
            InitializeComponent();
            
            // Setup Sound
            _soundManager = new SoundManager();

            // Setup Timer
            _gameTimer = new DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            _gameTimer.Tick += GameLoop;

            // Load Assets & Animations
            LoadAssets();

            // Menu Events
            StartButton.Click += StartGame;
            ExitButton.Click += (s, e) => Close();
            RestartButton.Click += StartGame;
            
            // New Menu Events
            OptionsButton.Click += (s, e) => { MenuOverlay.IsVisible = false; OptionsOverlay.IsVisible = true; };
            AboutButton.Click += (s, e) => { MenuOverlay.IsVisible = false; AboutOverlay.IsVisible = true; };
            OptionsBackButton.Click += SaveOptionsAndBack;
            AboutBackButton.Click += (s, e) => { AboutOverlay.IsVisible = false; MenuOverlay.IsVisible = true; };
            MenuButton.Click += (s, e) => { GameOverOverlay.IsVisible = false; MenuOverlay.IsVisible = true; };

            // Keyboard Events
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        private void LoadAssets()
        {
            try
            {
                if (File.Exists("Assets/Background.png"))
                    BackgroundImage.Source = new Bitmap("Assets/Background.png");
                
                if (File.Exists("Assets/Player.png"))
                    PlayerSprite.Source = new Bitmap("Assets/Player.png");

                if (File.Exists("Assets/Enemy.png"))
                    EnemySprite.Source = new Bitmap("Assets/Enemy.png");

                // Preload Animations
                _loadedAnimations["Player"] = LoadAnimationSet("Player");
                _loadedAnimations["Enemy"] = LoadAnimationSet("Enemy");
            }
            catch (Exception ex)
            {
                DebugText.Text = "Error loading assets: " + ex.Message;
            }
        }

        private Dictionary<ActionState, List<Bitmap>> LoadAnimationSet(string name)
        {
            var anims = new Dictionary<ActionState, List<Bitmap>>();
            foreach (ActionState state in Enum.GetValues(typeof(ActionState)))
            {
                var frames = new List<Bitmap>();
                int i = 0;
                // Try to load State_0.png, State_1.png...
                while (true)
                {
                    // Look for Assets/{Name}_{State}_{i}.png or similar pattern
                    // Assuming flat structure for simplicity: Assets/{Name}_{State}_{i}.png
                    string path = $"Assets/{name}_{state}_{i}.png";
                    
                    if (!File.Exists(path))
                    {
                        // Try folder structure: Assets/{Name}/{State}_{i}.png
                        path = $"Assets/{name}/{state}_{i}.png";
                    }

                    if (File.Exists(path))
                    {
                         try { frames.Add(new Bitmap(path)); } catch { }
                         i++;
                    }
                    else
                    {
                        break;
                    }
                }
                
                if (frames.Count > 0) 
                {
                    anims[state] = frames;
                }
            }
            return anims;
        }

        private void SaveOptionsAndBack(object? sender, RoutedEventArgs e)
        {
            // Save Difficulty
            if (DifficultyCombo.SelectedIndex == 0) GameSettings.Difficulty = DifficultyLevel.Easy;
            else if (DifficultyCombo.SelectedIndex == 1) GameSettings.Difficulty = DifficultyLevel.Normal;
            else GameSettings.Difficulty = DifficultyLevel.Hard;

            // Save Sound
            GameSettings.SoundEnabled = SoundCheck.IsChecked ?? true;

            OptionsOverlay.IsVisible = false;
            MenuOverlay.IsVisible = true;
        }

        private void StartGame(object? sender, RoutedEventArgs e)
        {
            MenuOverlay.IsVisible = false;
            GameOverOverlay.IsVisible = false;
            
            // Reset Game State
            _player = new Character(PlayerSprite, PlayerHealthBar, 100); // Start left
            if (_loadedAnimations.ContainsKey("Player")) _player.Animations = _loadedAnimations["Player"];

            _enemy = new Character(EnemySprite, EnemyHealthBar, 600);   // Start right
            if (_loadedAnimations.ContainsKey("Enemy")) _enemy.Animations = _loadedAnimations["Enemy"];
            
            _enemy.IsFacingRight = false; // Enemy faces left
            
            _timeLeft = 99;
            TimerText.Text = _timeLeft.ToString();
            _lastTimeTick = DateTime.Now;

            _isGameRunning = true;
            _gameTimer.Start();
            Focus(); // Ensure window has focus for key events
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            if (!_isGameRunning || _player == null || _enemy == null) return;

            // 1. Handle Input
            HandleInput();

            // 2. AI Logic
            HandleAI();

            // 3. Update Physics/State
            _player.Update();
            _enemy.Update();

            // 4. Collision / Combat Logic
            CheckCollisions();
            
            // Update Sound Cooldown
            if (_hitSoundCooldown > 0) _hitSoundCooldown--;

            // 5. Timer Logic
            if ((DateTime.Now - _lastTimeTick).TotalSeconds >= 1)
            {
                _timeLeft--;
                TimerText.Text = _timeLeft.ToString();
                _lastTimeTick = DateTime.Now;

                if (_timeLeft <= 0)
                {
                    if (_player.Health > _enemy.Health) EndGame("TIME UP! YOU WIN!", true);
                    else if (_player.Health < _enemy.Health) EndGame("TIME UP! YOU LOSE!", false);
                    else EndGame("TIME UP! DRAW!", false);
                }
            }

            // 6. Win/Loss Condition
            if (_player.Health <= 0) EndGame("YOU LOSE!", false);
            else if (_enemy.Health <= 0) EndGame("YOU WIN!", true);
        }

        private void PlaySound(SoundType type)
        {
            if (GameSettings.SoundEnabled)
            {
                _soundManager.Play(type);
            }
        }

        private void HandleInput()
        {
            if (_player == null) return;
            if (_player.State == ActionState.Hurt || _player.State == ActionState.Dead) return;

            // Movement
            if (_leftPressed) _player.Move(-1);
            else if (_rightPressed) _player.Move(1);
            else _player.VelocityX = 0; // Stop if no key

            // Actions
            if (_jumpPressed)
            {
                 if (_player.Jump()) PlaySound(SoundType.Jump);
            }
            
            // Attacks (Only if not already attacking)
            if (_player.State == ActionState.Idle || _player.State == ActionState.Walk)
            {
                if (_punchPressed)
                {
                    if (_player.Attack(ActionState.Punch)) PlaySound(SoundType.Punch);
                }
                else if (_kickPressed)
                {
                    if (_player.Attack(ActionState.Kick)) PlaySound(SoundType.Kick);
                }
                else if (_blockPressed)
                {
                    if (_player.Block()) PlaySound(SoundType.Block);
                }
            }
        }

        private void HandleAI()
        {
            if (_player == null || _enemy == null) return;
            if (_enemy.State == ActionState.Hurt || _enemy.State == ActionState.Dead) return;

            double distance = Math.Abs(_player.X - _enemy.X);
            Random rng = new Random();

            // AI Constants based on Difficulty
            int attackChance = 2; // Default 2%
            int blockChance = 1;  // Default 1%
            int reactionSpeed = 100; // Distance to react

            switch (GameSettings.Difficulty)
            {
                case DifficultyLevel.Easy:
                    attackChance = 1; // 1% chance
                    blockChance = 0;  // No block
                    reactionSpeed = 80;
                    break;
                case DifficultyLevel.Normal:
                    attackChance = 3; 
                    blockChance = 2;
                    reactionSpeed = 100;
                    break;
                case DifficultyLevel.Hard:
                    attackChance = 8; // Aggressive
                    blockChance = 5;  // Smart blocking
                    reactionSpeed = 150;
                    break;
            }

            // Simple AI Logic
            if (distance > reactionSpeed)
            {
                // Move towards player
                if (_player.X < _enemy.X) _enemy.Move(-1); // Move Left
                else _enemy.Move(1); // Move Right
            }
            else
            {
                // Close enough to engage
                _enemy.VelocityX = 0;
                
                // Defensive: If player is attacking, try to block (Hard mode only usually)
                if (GameSettings.Difficulty == DifficultyLevel.Hard && (_player.State == ActionState.Punch || _player.State == ActionState.Kick))
                {
                    if (rng.Next(0, 100) < 30) // 30% chance to react block
                    {
                         if (_enemy.Block()) PlaySound(SoundType.Block);
                         return;
                    }
                }

                // Offensive
                if (rng.Next(0, 100) < attackChance) 
                {
                    if (rng.Next(0, 2) == 0)
                    {
                        if (_enemy.Attack(ActionState.Punch)) PlaySound(SoundType.Punch);
                    }
                    else
                    {
                        if (_enemy.Attack(ActionState.Kick)) PlaySound(SoundType.Kick);
                    }
                }
                else if (rng.Next(0, 100) < blockChance) 
                {
                    if (_enemy.Block()) PlaySound(SoundType.Block);
                }
            }
        }

        private void CheckCollisions()
        {
            if (_player == null || _enemy == null) return;

            // Player attacks Enemy
            // Hit logic: Must be in attack state, and in the "active" part of the animation (e.g. middle of the swing)
            // Using StateTimer: Starts at 20. Active usually around 15-5.
            if ((_player.State == ActionState.Punch || _player.State == ActionState.Kick) && _player.StateTimer < 15 && _player.StateTimer > 5) 
            {
                 if (Math.Abs(_player.X - _enemy.X) < 80 && Math.Abs(_player.Y - _enemy.Y) < 50)
                 {
                     // Only hit if cooldown allows (to prevent multi-hit per punch)
                     if (_hitSoundCooldown <= 0)
                     {
                         _enemy.TakeDamage(10.0); // Standard damage
                         if (_enemy.State == ActionState.Block) PlaySound(SoundType.Block);
                         else PlaySound(SoundType.Hit);
                         _hitSoundCooldown = 15; // Global hit cooldown
                     }
                 }
            }

            // Enemy attacks Player
            if ((_enemy.State == ActionState.Punch || _enemy.State == ActionState.Kick) && _enemy.StateTimer < 15 && _enemy.StateTimer > 5)
            {
                 if (Math.Abs(_enemy.X - _player.X) < 80 && Math.Abs(_enemy.Y - _player.Y) < 50)
                 {
                     if (_hitSoundCooldown <= 0)
                     {
                         _player.TakeDamage(10.0);
                         if (_player.State == ActionState.Block) PlaySound(SoundType.Block);
                         else PlaySound(SoundType.Hit);
                         _hitSoundCooldown = 15;
                     }
                 }
            }
            
            // Push characters apart (simple collision)
            if (Math.Abs(_player.X - _enemy.X) < 40)
            {
                if (_player.X < _enemy.X) 
                {
                    _player.X -= 2;
                    _enemy.X += 2;
                }
                else
                {
                    _player.X += 2;
                    _enemy.X -= 2;
                }
            }
        }

        private void EndGame(string message, bool isWin)
        {
            _isGameRunning = false;
            _gameTimer.Stop();
            WinnerText.Text = message;
            GameOverOverlay.IsVisible = true;
            
            PlaySound(isWin ? SoundType.Win : SoundType.Lose);
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left: _leftPressed = true; break;
                case Key.Right: _rightPressed = true; break;
                case Key.Up: _jumpPressed = true; break;
                case Key.Z: _punchPressed = true; break;
                case Key.X: _kickPressed = true; break;
                case Key.C: _blockPressed = true; break;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left: _leftPressed = false; break;
                case Key.Right: _rightPressed = false; break;
                case Key.Up: _jumpPressed = false; break;
                case Key.Z: _punchPressed = false; break;
                case Key.X: _kickPressed = false; break;
                case Key.C: _blockPressed = false; break;
            }
        }
    }
}