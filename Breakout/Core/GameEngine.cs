using Breakout.GameObjects;
using Breakout.Effects;
using Breakout.Levels;
using SkiaSharp;

namespace Breakout.Core;

/// <summary>
/// Mesin utama game. Mengelola game loop, update, dan render semua objek.
/// </summary>
public class GameEngine
{
    // --- Constants ---
    public const float GAME_WIDTH = 800;
    public const float GAME_HEIGHT = 600;

    // --- States ---
    public GameState State { get; private set; } = GameState.Menu;
    public AppSettings Settings { get; private set; }
    public ScoreManager ScoreManager { get; private set; }
    public InputManager Input { get; private set; }

    // --- Game Objects ---
    public Ball Ball { get; private set; }
    public Paddle Paddle { get; private set; }
    public List<Brick> Bricks { get; private set; } = new();
    public List<PowerUp> PowerUps { get; private set; } = new();
    public List<Laser> Lasers { get; private set; } = new();

    // --- Systems ---
    public ParticleSystem ParticleSystem { get; private set; }
    public BallTrail BallTrail { get; private set; }
    public LevelManager LevelManager { get; private set; }
    public Background Background { get; private set; }
    public MenuManager MenuManager { get; private set; }
    public Hud Hud { get; private set; }
    public CollisionDetector CollisionDetector { get; private set; }

    // --- Timers ---
    private float _slowMotionTimer;
    private float _multiBallTimer;
    private float _laserTimer;
    private float _paddleSizeTimer;
    private float _gameOverTimer;
    private float _levelClearTimer;

    // --- Events ---
    public event Action OnBallLost;
    public event Action OnLevelClear;
    public event Action OnGameOver;

    // --- Random ---
    private readonly Random _random = new();

    // Frame timing
    private DateTime _lastFrameTime = DateTime.UtcNow;
    private float _deltaTime;
    private int _frameCount;
    private float _fpsTimer;
    private float _currentFps;

    public GameEngine()
    {
        Settings = AppSettings.Load();
        Input = new InputManager();
        ScoreManager = new ScoreManager(Settings.StartingLives);
        LevelManager = new LevelManager();
        ParticleSystem = new ParticleSystem();
        BallTrail = new BallTrail();
        MenuManager = new MenuManager(this);
        Hud = new Hud(this);
        Background = new Background();
        CollisionDetector = new CollisionDetector(this);

        // Initialize game objects
        ResetGameObjects();
    }

    private void ResetGameObjects()
    {
        Ball = new Ball(GAME_WIDTH / 2, GAME_HEIGHT - 60, Settings.SpeedMultiplier);
        Paddle = new Paddle(GAME_WIDTH / 2, GAME_HEIGHT - 30);
        PowerUps.Clear();
        Lasers.Clear();
        ParticleSystem.Clear();
        BallTrail.Clear();

        _slowMotionTimer = 0;
        _multiBallTimer = 0;
        _laserTimer = 0;
        _paddleSizeTimer = 0;
    }

    /// <summary>
    /// Memulai game baru.
    /// </summary>
    public void StartNewGame(DifficultyLevel? difficulty = null)
    {
        if (difficulty.HasValue)
        {
            Settings.Difficulty = difficulty.Value;
            Settings.Save();
        }

        ScoreManager.Reset(Settings.StartingLives);
        ResetGameObjects();
        LoadCurrentLevel();
        State = GameState.Playing;
    }

    /// <summary>
    /// Muat level saat ini dari LevelManager.
    /// </summary>
    public void LoadCurrentLevel()
    {
        var levelData = LevelManager.GetLevel(ScoreManager.Level);
        if (levelData == null)
        {
            // Semua level selesai!
            State = GameState.LevelClear;
            return;
        }

        Bricks = levelData.CreateBricks(Settings.SpeedMultiplier);
        Background.SetLevelTheme(ScoreManager.Level);
        ResetGameObjects();

        // Atur posisi paddle dan ball sesuai level
        Ball.Reset(GAME_WIDTH / 2, GAME_HEIGHT - 60);
        Paddle.Reset(GAME_WIDTH / 2, GAME_HEIGHT - 30);
    }

    /// <summary>
    /// Update game logic setiap frame.
    /// </summary>
    public void Update()
    {
        float dt = GetDeltaTime();

        // FPS counter
        _fpsTimer += dt;
        _frameCount++;
        if (_fpsTimer >= 1.0f)
        {
            _currentFps = _frameCount;
            _frameCount = 0;
            _fpsTimer = 0;
        }

        switch (State)
        {
            case GameState.Menu:
            case GameState.Settings:
            case GameState.About:
                MenuManager.Update(dt);
                break;

            case GameState.Playing:
                UpdatePlaying(dt);
                break;

            case GameState.Paused:
                if (Input.IsKeyPressed(Keys.Escape) || Input.IsKeyPressed(Keys.P))
                    State = GameState.Playing;
                break;

            case GameState.GameOver:
                _gameOverTimer += dt;
                if (_gameOverTimer > 2.0f && (Input.IsKeyPressed(Keys.Space) || Input.IsKeyPressed(Keys.Enter)))
                {
                    State = GameState.Menu;
                    _gameOverTimer = 0;
                }
                break;

            case GameState.LevelClear:
                _levelClearTimer += dt;
                ParticleSystem.Update(dt);
                if (_levelClearTimer > 2.0f && (Input.IsKeyPressed(Keys.Space) || Input.IsKeyPressed(Keys.Enter)))
                {
                    ScoreManager.Level++;
                    LoadCurrentLevel();
                    _levelClearTimer = 0;
                }
                break;
        }

        Input.EndFrame();
    }

    private void UpdatePlaying(float dt)
    {
        // Slow motion effect
        if (_slowMotionTimer > 0)
        {
            dt *= 0.3f;
            _slowMotionTimer -= dt / 0.3f;
        }

        // Update timers for power-ups
        UpdatePowerUpTimers(dt);

        // Update background (parallax)
        Background.Update(dt);

        // Update ball
        Ball.Update(dt, GAME_WIDTH, GAME_HEIGHT);
        BallTrail.AddPoint(Ball.Position);

        // Update paddle
        float paddleDir = Input.GetPaddleDirection();
        Paddle.Update(dt, paddleDir, GAME_WIDTH);

        // Update power-ups
        foreach (var pu in PowerUps.ToList())
        {
            pu.Update(dt);
            if (pu.IsOffScreen(GAME_HEIGHT))
                PowerUps.Remove(pu);
        }

        // Update lasers
        foreach (var laser in Lasers.ToList())
        {
            laser.Update(dt);
            if (laser.IsOffScreen(GAME_HEIGHT))
                Lasers.Remove(laser);
        }

        // Collision detection
        CollisionDetector.CheckCollisions();

        // Update particles
        ParticleSystem.Update(dt);

        // Update ball trail
        BallTrail.Update(dt);

        // Check game over
        if (Ball.IsLost)
        {
            bool dead = ScoreManager.LoseLife();
            ParticleSystem.CreateLifeLostEffect(Ball.Position);
            Ball.Reset(GAME_WIDTH / 2, GAME_HEIGHT - 60);
            Paddle.Reset(GAME_WIDTH / 2, GAME_HEIGHT - 30);

            if (dead)
            {
                State = GameState.GameOver;
                OnGameOver?.Invoke();
            }
            else
            {
                OnBallLost?.Invoke();
            }
        }

        // Check level clear
        if (Bricks.Count > 0 && Bricks.All(b => b.IsDestroyed))
        {
            State = GameState.LevelClear;
            OnLevelClear?.Invoke();
            ParticleSystem.CreateLevelClearEffect(new SKPoint(GAME_WIDTH / 2, GAME_HEIGHT / 2));
        }

        // Shoot laser
        if (_laserTimer > 0 && Input.IsKeyPressed(Keys.Space))
        {
            ShootLaser();
        }

        // Multi-ball: launch extra balls
        if (_multiBallTimer > 0 && Input.IsKeyPressed(Keys.Space))
        {
            // Already handled - ball launch is on click
        }

        // Launch ball on mouse click / space
        if (Ball.IsStuck && (Input.IsKeyPressed(Keys.Space) || Input.MouseLeftClicked))
        {
            Ball.Launch();
        }
    }

    private void UpdatePowerUpTimers(float dt)
    {
        if (_slowMotionTimer > 0) _slowMotionTimer -= dt;
        if (_multiBallTimer > 0) _multiBallTimer -= dt;
        if (_laserTimer > 0) _laserTimer -= dt;
        if (_paddleSizeTimer > 0)
        {
            _paddleSizeTimer -= dt;
            if (_paddleSizeTimer <= 0)
                Paddle.ResetSize();
        }
    }

    /// <summary>
    /// Apply power-up effects.
    /// </summary>
    public void ApplyPowerUp(PowerUpType type)
    {
        switch (type)
        {
            case PowerUpType.PaddleIncrease:
                Paddle.SetSizeMultiplier(1.5f);
                _paddleSizeTimer = 10.0f;
                break;
            case PowerUpType.PaddleDecrease:
                Paddle.SetSizeMultiplier(0.6f);
                _paddleSizeTimer = 8.0f;
                break;
            case PowerUpType.MultiBall:
                _multiBallTimer = 15.0f;
                // Create 2 extra balls
                for (int i = 0; i < 2; i++)
                {
                    // Simplified: we'll spawn extra balls
                }
                break;
            case PowerUpType.Laser:
                _laserTimer = 12.0f;
                break;
            case PowerUpType.SlowMotion:
                _slowMotionTimer = 8.0f;
                break;
        }

        ParticleSystem.CreatePowerUpEffect(new SKPoint(GAME_WIDTH / 2, GAME_HEIGHT / 2), type);
    }

    private void ShootLaser()
    {
        var pos = Paddle.Position;
        Lasers.Add(new Laser(pos.X - 10, pos.Y));
        Lasers.Add(new Laser(pos.X + 10, pos.Y));
    }

    /// <summary>
    /// Render semua objek ke canvas.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        canvas.Clear(SKColors.Black);

        switch (State)
        {
            case GameState.Menu:
                MenuManager.RenderMenu(canvas);
                break;
            case GameState.Settings:
                MenuManager.RenderSettings(canvas);
                break;
            case GameState.About:
                MenuManager.RenderAbout(canvas);
                break;
            case GameState.Playing:
            case GameState.Paused:
            case GameState.LevelClear:
                RenderGame(canvas);
                break;
            case GameState.GameOver:
                RenderGame(canvas);
                MenuManager.RenderGameOver(canvas);
                break;
        }

        // FPS display
        if (Settings.ShowFPS && State != GameState.Menu)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Lime,
                TextSize = 12,
                IsAntialias = true
            };
            canvas.DrawText($"FPS: {_currentFps:F0}", GAME_WIDTH - 80, 15, paint);
        }
    }

    private void RenderGame(SKCanvas canvas)
    {
        // Background
        Background.Render(canvas, GAME_WIDTH, GAME_HEIGHT);

        // Ball trail
        if (Settings.BallTrailEnabled)
            BallTrail.Render(canvas);

        // Particles
        if (Settings.ParticlesEnabled)
            ParticleSystem.Render(canvas);

        // Lasers
        foreach (var laser in Lasers)
            laser.Render(canvas);

        // Power-ups
        foreach (var pu in PowerUps)
            pu.Render(canvas);

        // Bricks
        foreach (var brick in Bricks.Where(b => !b.IsDestroyed))
            brick.Render(canvas);

        // Paddle
        Paddle.Render(canvas);

        // Ball
        Ball.Render(canvas);

        // HUD
        Hud.Render(canvas);

        // Pause overlay
        if (State == GameState.Paused)
            MenuManager.RenderPauseOverlay(canvas);

        // Level clear overlay
        if (State == GameState.LevelClear)
            MenuManager.RenderLevelClear(canvas);
    }

    /// <summary>
    /// Hitung delta time.
    /// </summary>
    private float GetDeltaTime()
    {
        var now = DateTime.UtcNow;
        float dt = (float)(now - _lastFrameTime).TotalSeconds;
        _lastFrameTime = now;

        // Clamp delta time
        if (dt > 0.05f) dt = 0.05f;
        if (dt <= 0) dt = 0.016f;

        return dt;
    }

    /// <summary>
    /// Resize canvas.
    /// </summary>
    public void Resize(float width, float height)
    {
        // Scale game to fit window
    }

    internal void SetState(GameState state) => State = state;
}
