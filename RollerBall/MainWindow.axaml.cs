using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using RollerBall.Models;
using RollerBall.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RollerBall;

public partial class MainWindow : Window
{
    private DispatcherTimer _gameTimer;
    private List<PathPoint> _pathPoints = new List<PathPoint>();
    private List<Ball> _trackBalls = new List<Ball>();
    private List<Ball> _projectiles = new List<Ball>();
    private Ball? _currentAmmo;
    private Ball? _nextAmmo;
    private Point _shooterPosition;
    private double _shooterAngle;
    
    private int _score = 0;
    private int _level = 1;
    private bool _isGameOver = false;
    private bool _isPaused = true;
    
    private SoundManager? _soundManager;
    
    // Config
    private const double BALL_RADIUS = 15;
    private const double PATH_SPEED = 1.0; 
    private const double PROJECTILE_SPEED = 15.0;
    private const double SPAWN_INTERVAL = 40; 
    private int _spawnTimer = 0;
    private int _ballsToSpawn = 50;
    private int _spawnedCount = 0;
    private Random _rng = new Random();

    public MainWindow()
    {
        InitializeComponent();
        
        // Load background
        this.Opened += (s, e) => {
            try 
            {
                if (File.Exists("Assets/background.png"))
                {
                    BackgroundImg.Source = new Bitmap("Assets/background.png");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not load background: " + ex.Message);
            }
            InitializeGameEngine();
        };

        // Input
        this.PointerMoved += OnPointerMoved;
        this.PointerPressed += OnPointerPressed;
    }

    private void InitializeGameEngine()
    {
        _soundManager = new SoundManager();
        GeneratePath();
        DrawTrack(); // Draw the track
        _shooterPosition = new Point(400, 300);

        _gameTimer = new DispatcherTimer();
        _gameTimer.Interval = TimeSpan.FromMilliseconds(16);
        _gameTimer.Tick += GameLoop;
    }

    private void DrawTrack()
    {
        TrackCanvas.Children.Clear();
        
        if (_pathPoints.Count < 2) return;

        var points = _pathPoints.Select(p => p.Position).ToList();

        // Outer border
        var borderLine = new Avalonia.Controls.Shapes.Polyline
        {
            Stroke = Brushes.Black,
            StrokeThickness = (BALL_RADIUS * 2) + 4,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
            Points = points
        };
        TrackCanvas.Children.Add(borderLine);

        // Inner track
        var trackLine = new Avalonia.Controls.Shapes.Polyline
        {
            Stroke = new SolidColorBrush(Color.Parse("#333333")), 
            StrokeThickness = BALL_RADIUS * 2,
            StrokeLineCap = PenLineCap.Round,
            StrokeJoin = PenLineJoin.Round,
            Points = points
        };
        TrackCanvas.Children.Add(trackLine);
        
        // Center line
        var centerLine = new Avalonia.Controls.Shapes.Polyline
        {
            Stroke = new SolidColorBrush(Color.Parse("#555555")),
            StrokeThickness = 2,
            StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 5, 5 },
            Points = points
        };
        TrackCanvas.Children.Add(centerLine);
        
        // End hole
        var lastPt = _pathPoints.Last().Position;
        var hole = new Avalonia.Controls.Shapes.Ellipse
        {
            Width = BALL_RADIUS * 3,
            Height = BALL_RADIUS * 3,
            Fill = Brushes.Black
        };
        Canvas.SetLeft(hole, lastPt.X - (BALL_RADIUS * 1.5));
        Canvas.SetTop(hole, lastPt.Y - (BALL_RADIUS * 1.5));
        TrackCanvas.Children.Add(hole);
    }

    private void OnNewGameClick(object? sender, RoutedEventArgs e)
    {
        StartGame();
    }

    private void OnExitClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
    
    // --- Menu Handling ---

    private void OnOptionsClick(object? sender, RoutedEventArgs e)
    {
        MenuOverlay.IsVisible = false;
        OptionsOverlay.IsVisible = true;
    }

    private void OnAboutClick(object? sender, RoutedEventArgs e)
    {
        MenuOverlay.IsVisible = false;
        AboutOverlay.IsVisible = true;
    }

    private void OnOptionsBackClick(object? sender, RoutedEventArgs e)
    {
        OptionsOverlay.IsVisible = false;
        MenuOverlay.IsVisible = true;
    }

    private void OnAboutBackClick(object? sender, RoutedEventArgs e)
    {
        AboutOverlay.IsVisible = false;
        MenuOverlay.IsVisible = true;
    }
    
    private void OnSoundChecked(object? sender, RoutedEventArgs e)
    {
        if (_soundManager != null) _soundManager.IsSoundEnabled = true;
    }
    
    private void OnSoundUnchecked(object? sender, RoutedEventArgs e)
    {
        if (_soundManager != null) _soundManager.IsSoundEnabled = false;
    }

    private void OnMenuClick(object? sender, RoutedEventArgs e)
    {
        MenuOverlay.IsVisible = true;
        GameOverOverlay.IsVisible = false;
        _isPaused = true;
        _gameTimer.Stop();
    }

    private void StartGame()
    {
        _score = 0;
        _level = 1;
        _spawnedCount = 0;
        _ballsToSpawn = 30 + (_level * 10);
        _trackBalls.Clear();
        _projectiles.Clear();
        _isGameOver = false;
        _isPaused = false;
        
        ScoreText.Text = $"Score: {_score}";
        LevelText.Text = $"Level: {_level}";
        
        GenerateAmmo();
        MenuOverlay.IsVisible = false;
        GameOverOverlay.IsVisible = false;
        
        _gameTimer.Start();
    }

    private void GeneratePath()
    {
        _pathPoints.Clear();
        // Spiral formula
        double cx = 400;
        double cy = 300;
        double angleStep = 0.05;
        double maxAngle = 6 * Math.PI; 
        double a = 50; 
        double b = 25; 

        double currentDist = 0;
        Point prevPt = new Point(cx + (a + b * maxAngle) * Math.Cos(maxAngle), 
                                 cy + (a + b * maxAngle) * Math.Sin(maxAngle));

        for (double theta = maxAngle; theta >= 0; theta -= angleStep)
        {
            double r = a + b * theta;
            double x = cx + r * Math.Cos(theta);
            double y = cy + r * Math.Sin(theta);
            Point pt = new Point(x, y);
            
            double segDist = Math.Sqrt(Math.Pow(pt.X - prevPt.X, 2) + Math.Pow(pt.Y - prevPt.Y, 2));
            currentDist += segDist;
            
            _pathPoints.Add(new PathPoint { Position = pt, TotalDistance = currentDist });
            prevPt = pt;
        }
    }

    private void GenerateAmmo()
    {
        _currentAmmo = new Ball { Color = GetRandomColor(), Position = _shooterPosition, Radius = BALL_RADIUS };
        _nextAmmo = new Ball { Color = GetRandomColor(), Position = new Point(700, 50), Radius = BALL_RADIUS };
    }
    
    private BallColor GetRandomColor()
    {
        var values = Enum.GetValues(typeof(BallColor));
        return (BallColor)values.GetValue(_rng.Next(values.Length))!;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPaused) return;
        
        var pos = e.GetPosition(this);
        double dx = pos.X - _shooterPosition.X;
        double dy = pos.Y - _shooterPosition.Y;
        _shooterAngle = Math.Atan2(dy, dx);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_isPaused || _isGameOver || _currentAmmo == null || _nextAmmo == null) return;
        
        // Shoot
        _soundManager?.PlayShoot();
        var projectile = new Ball
        {
            Color = _currentAmmo.Color,
            Position = _shooterPosition,
            Radius = BALL_RADIUS,
            IsProjectile = true,
            Velocity = new Vector(Math.Cos(_shooterAngle) * PROJECTILE_SPEED, Math.Sin(_shooterAngle) * PROJECTILE_SPEED)
        };
        _projectiles.Add(projectile);
        
        // Cycle ammo
        _currentAmmo = _nextAmmo;
        _currentAmmo.Position = _shooterPosition;
        _nextAmmo = new Ball { Color = GetRandomColor(), Position = new Point(700, 50), Radius = BALL_RADIUS };
    }

    private void GameLoop(object? sender, EventArgs e)
    {
        if (_isPaused) return;

        UpdateGame();
        RenderGame();
    }

    private void UpdateGame()
    {
        // 1. Spawn Balls
        if (_spawnedCount < _ballsToSpawn)
        {
            _spawnTimer++;
            if (_spawnTimer >= SPAWN_INTERVAL)
            {
                _spawnTimer = 0;
                var newBall = new Ball 
                { 
                    Color = GetRandomColor(), 
                    Distance = 0, 
                    Radius = BALL_RADIUS 
                };
                _trackBalls.Add(newBall);
                _spawnedCount++;
            }
        }
        else if (_trackBalls.Count == 0 && _projectiles.Count == 0)
        {
            LevelComplete();
            return;
        }

        // 2. Move Track Balls
        if (_trackBalls.Count > 0)
        {
             _trackBalls.Sort((a, b) => b.Distance.CompareTo(a.Distance));
             
             // Move lead
             _trackBalls[0].Distance += PATH_SPEED;
             
             // Move others
             for (int i = 1; i < _trackBalls.Count; i++)
             {
                 Ball leader = _trackBalls[i-1];
                 Ball follower = _trackBalls[i];
                 
                 double desiredDist = leader.Distance - (BALL_RADIUS * 2);
                 
                 follower.Distance += PATH_SPEED;
                 
                 if (follower.Distance > desiredDist)
                 {
                     follower.Distance = desiredDist;
                 }
             }
        }

        // Update Position
        foreach (var ball in _trackBalls)
        {
            ball.Position = GetPointAtDistance(ball.Distance);
            if (ball.Distance >= _pathPoints.Last().TotalDistance)
            {
                GameOver();
                return;
            }
        }

        // 3. Move Projectiles
        for (int i = _projectiles.Count - 1; i >= 0; i--)
        {
            var p = _projectiles[i];
            p.Position = new Point(p.Position.X + p.Velocity.X, p.Position.Y + p.Velocity.Y);
            
            if (p.Position.X < 0 || p.Position.X > Bounds.Width || p.Position.Y < 0 || p.Position.Y > Bounds.Height)
            {
                _projectiles.RemoveAt(i);
                continue;
            }
            
            bool hit = false;
            for (int j = 0; j < _trackBalls.Count; j++)
            {
                var tBall = _trackBalls[j];
                double dist = Math.Sqrt(Math.Pow(p.Position.X - tBall.Position.X, 2) + Math.Pow(p.Position.Y - tBall.Position.Y, 2));
                
                if (dist < BALL_RADIUS * 2)
                {
                    InsertBall(p, j);
                    _projectiles.RemoveAt(i);
                    hit = true;
                    break;
                }
            }
        }
    }

    private Point GetPointAtDistance(double d)
    {
        if (_pathPoints.Count == 0) return new Point(0,0);
        if (d <= 0) return _pathPoints[0].Position;
        if (d >= _pathPoints.Last().TotalDistance) return _pathPoints.Last().Position;

        for (int i = 0; i < _pathPoints.Count - 1; i++)
        {
            if (d >= _pathPoints[i].TotalDistance && d <= _pathPoints[i+1].TotalDistance)
            {
                var p1 = _pathPoints[i];
                var p2 = _pathPoints[i+1];
                double ratio = (d - p1.TotalDistance) / (p2.TotalDistance - p1.TotalDistance);
                return new Point(
                    p1.Position.X + ratio * (p2.Position.X - p1.Position.X),
                    p1.Position.Y + ratio * (p2.Position.Y - p1.Position.Y)
                );
            }
        }
        return _pathPoints.Last().Position;
    }

    private void InsertBall(Ball projectile, int hitIndex)
    {
        _soundManager?.PlayPop();
        Ball newBall = new Ball 
        { 
            Color = projectile.Color, 
            Radius = BALL_RADIUS,
            Distance = _trackBalls[hitIndex].Distance 
        };
        
        _trackBalls.Insert(hitIndex + 1, newBall);
        
        double shift = BALL_RADIUS * 2;
        for (int k = hitIndex + 1; k < _trackBalls.Count; k++)
        {
            _trackBalls[k].Distance -= shift; 
        }
        
        CheckMatches(hitIndex + 1);
    }

    private void CheckMatches(int index)
    {
        if (index < 0 || index >= _trackBalls.Count) return;
        
        var color = _trackBalls[index].Color;
        int count = 1;
        int start = index;
        int end = index;
        
        for (int i = index - 1; i >= 0; i--)
        {
            if (_trackBalls[i].Color == color) { count++; start = i; }
            else break;
        }
        
        for (int i = index + 1; i < _trackBalls.Count; i++)
        {
            if (_trackBalls[i].Color == color) { count++; end = i; }
            else break;
        }
        
        if (count >= 3)
        {
            _soundManager?.PlayExplode();
            int removeCount = end - start + 1;
            _score += removeCount * 100;
            ScoreText.Text = $"Score: {_score}";
            _trackBalls.RemoveRange(start, removeCount);
            
            // Simple Gap Close (Instant Teleport of back segment)
            if (start < _trackBalls.Count && start > 0)
            {
                 // Move start (which was end+1) to be behind start-1
                 Ball leadBeforeGap = _trackBalls[start - 1];
                 Ball headOfBackSegment = _trackBalls[start];
                 
                 double diff = leadBeforeGap.Distance - (BALL_RADIUS * 2) - headOfBackSegment.Distance;
                 // Add diff to all back segment
                 for(int k=start; k < _trackBalls.Count; k++)
                 {
                     _trackBalls[k].Distance += diff;
                 }
            }
        }
    }

    private void LevelComplete()
    {
        _gameTimer.Stop();
        _level++;
        StartGame(); 
    }

    private void GameOver()
    {
        _soundManager?.PlayGameOver();
        _gameTimer.Stop();
        _isGameOver = true;
        GameOverText.Text = "GAME OVER";
        FinalScoreText.Text = $"Final Score: {_score}";
        GameOverOverlay.IsVisible = true;
    }

    private void RenderGame()
    {
        GameCanvas.Children.Clear();

        // Balls
        foreach (var ball in _trackBalls)
        {
            var ellipse = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = ball.Radius * 2,
                Height = ball.Radius * 2,
                Fill = ball.GetBrush(),
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(ellipse, ball.Position.X - ball.Radius);
            Canvas.SetTop(ellipse, ball.Position.Y - ball.Radius);
            GameCanvas.Children.Add(ellipse);
        }

        // Projectiles
        foreach (var p in _projectiles)
        {
            var ellipse = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = p.Radius * 2,
                Height = p.Radius * 2,
                Fill = p.GetBrush(),
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
            Canvas.SetLeft(ellipse, p.Position.X - p.Radius);
            Canvas.SetTop(ellipse, p.Position.Y - p.Radius);
            GameCanvas.Children.Add(ellipse);
        }

        // Shooter
        var shooterShape = new Avalonia.Controls.Shapes.Ellipse
        {
            Width = 50,
            Height = 50,
            Fill = Brushes.DarkGray,
            Stroke = Brushes.Gold,
            StrokeThickness = 3
        };
        Canvas.SetLeft(shooterShape, _shooterPosition.X - 25);
        Canvas.SetTop(shooterShape, _shooterPosition.Y - 25);
        GameCanvas.Children.Add(shooterShape);
        
        var dirLine = new Avalonia.Controls.Shapes.Line
        {
             StartPoint = _shooterPosition,
             EndPoint = new Point(_shooterPosition.X + Math.Cos(_shooterAngle)*40, _shooterPosition.Y + Math.Sin(_shooterAngle)*40),
             Stroke = Brushes.Gold,
             StrokeThickness = 3
        };
        GameCanvas.Children.Add(dirLine);
        
        if (_currentAmmo != null)
        {
            var ammoShape = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = _currentAmmo.Radius * 2,
                Height = _currentAmmo.Radius * 2,
                Fill = _currentAmmo.GetBrush()
            };
            Canvas.SetLeft(ammoShape, _shooterPosition.X - _currentAmmo.Radius);
            Canvas.SetTop(ammoShape, _shooterPosition.Y - _currentAmmo.Radius);
            GameCanvas.Children.Add(ammoShape);
        }
        
        if (_nextAmmo != null)
        {
             var nextShape = new Avalonia.Controls.Shapes.Ellipse
            {
                Width = _nextAmmo.Radius * 2,
                Height = _nextAmmo.Radius * 2,
                Fill = _nextAmmo.GetBrush(),
                Opacity = 0.5
            };
            Canvas.SetLeft(nextShape, _nextAmmo.Position.X - _nextAmmo.Radius);
            Canvas.SetTop(nextShape, _nextAmmo.Position.Y - _nextAmmo.Radius);
            GameCanvas.Children.Add(nextShape);
        }
    }
}