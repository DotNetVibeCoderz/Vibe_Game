using Avalonia;
using Avalonia.Media;

namespace RollerBall.Models;

public enum BallColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Cyan
}

public class Ball
{
    public BallColor Color { get; set; }
    public double Distance { get; set; } // Distance along the path curve
    public Point Position { get; set; }
    public double Radius { get; set; } = 15;
    public bool IsActive { get; set; } = true;
    public bool IsProjectile { get; set; } = false;
    
    // For projectiles
    public Vector Velocity { get; set; }
    
    public IBrush GetBrush()
    {
        return Color switch
        {
            BallColor.Red => Brushes.Red,
            BallColor.Blue => Brushes.Blue,
            BallColor.Green => Brushes.Green,
            BallColor.Yellow => Brushes.Yellow,
            BallColor.Purple => Brushes.Purple,
            BallColor.Cyan => Brushes.Cyan,
            _ => Brushes.Gray
        };
    }
}

public class PathPoint
{
    public Point Position { get; set; }
    public double TotalDistance { get; set; }
}