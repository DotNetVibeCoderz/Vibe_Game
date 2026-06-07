using Breakout.GameObjects;
using Breakout.Levels;
using SkiaSharp;

namespace Breakout.Core;

/// <summary>
/// Sistem deteksi tabrakan antara ball, paddle, bricks, dan objek lainnya.
/// </summary>
public class CollisionDetector
{
    private readonly GameEngine _engine;
    private readonly Random _random = new();

    public CollisionDetector(GameEngine engine)
    {
        _engine = engine;
    }

    /// <summary>
    /// Periksa semua kemungkinan tabrakan.
    /// </summary>
    public void CheckCollisions()
    {
        var ball = _engine.Ball;
        var paddle = _engine.Paddle;
        var bricks = _engine.Bricks;
        var lasers = _engine.Lasers;

        // Ball vs Paddle
        if (!ball.IsLost && !ball.IsStuck)
            CheckBallPaddle(ball, paddle);

        // Ball vs Bricks
        if (!ball.IsLost && !ball.IsStuck)
        {
            foreach (var brick in bricks.Where(b => !b.IsDestroyed))
            {
                if (CheckBallBrick(ball, brick))
                    break; // Only one brick hit per frame
            }
        }

        // Laser vs Bricks
        foreach (var laser in lasers.ToList())
        {
            foreach (var brick in bricks.Where(b => !b.IsDestroyed))
            {
                if (CheckLaserBrick(laser, brick))
                {
                    lasers.Remove(laser);
                    break;
                }
            }
        }

        // Ball vs Power-ups
        foreach (var pu in _engine.PowerUps.ToList())
        {
            if (!pu.IsCollected && CheckBallPowerUp(ball, pu))
            {
                pu.Collect();
                _engine.ApplyPowerUp(pu.Type);
                _engine.ScoreManager.AddScore(50);
            }
        }

        // Paddle vs Power-ups
        foreach (var pu in _engine.PowerUps.ToList())
        {
            if (!pu.IsCollected && CheckRectCollision(
                paddle.Bounds, pu.Bounds))
            {
                pu.Collect();
                _engine.ApplyPowerUp(pu.Type);
                _engine.ScoreManager.AddScore(50);
            }
        }
    }

    /// <summary>
    /// Deteksi tabrakan ball dengan paddle.
    /// </summary>
    private void CheckBallPaddle(Ball ball, Paddle paddle)
    {
        if (!CheckCircleRectCollision(ball.Position, ball.Radius, paddle.Bounds))
            return;

        // Hitung sudut pantulan berdasarkan posisi tabrakan
        float hitPos = (ball.Position.X - paddle.Bounds.Left) / paddle.Bounds.Width;
        hitPos = Math.Clamp(hitPos, 0, 1);

        // Pukul ke kiri jika hitPos < 0.5, ke kanan jika > 0.5
        float angle = -MathF.PI / 4 + hitPos * (MathF.PI / 2); // -45° sampai 225° dari atas
        float speed = ball.Speed;

        ball.Velocity = new SKPoint(
            MathF.Cos(angle) * speed,
            MathF.Sin(angle) * speed
        );

        // Pastikan ball bergerak ke atas
        if (ball.Velocity.Y > 0)
            ball.Velocity = new SKPoint(ball.Velocity.X, -ball.Velocity.Y);

        // Posisi ball di atas paddle
        ball.Position = new SKPoint(ball.Position.X, paddle.Bounds.Top - ball.Radius - 1);

        // Efek partikel saat tabrakan
        _engine.ParticleSystem.CreatePaddleHitEffect(ball.Position);
        _engine.ScoreManager.ResetCombo();
    }

    /// <summary>
    /// Deteksi tabrakan ball dengan brick.
    /// </summary>
    private bool CheckBallBrick(Ball ball, Brick brick)
    {
        if (!CheckCircleRectCollision(ball.Position, ball.Radius, brick.Bounds))
            return false;

        // Tentukan sisi tabrakan
        var overlap = GetOverlap(ball.Position, ball.Radius, brick.Bounds);

        // Pantulkan bola berdasarkan arah tabrakan
        if (overlap.X < overlap.Y)
        {
            ball.Velocity = new SKPoint(-ball.Velocity.X, ball.Velocity.Y);
            if (ball.Position.X < brick.Bounds.MidX)
                ball.Position = new SKPoint(brick.Bounds.Left - ball.Radius - 1, ball.Position.Y);
            else
                ball.Position = new SKPoint(brick.Bounds.Right + ball.Radius + 1, ball.Position.Y);
        }
        else
        {
            ball.Velocity = new SKPoint(ball.Velocity.X, -ball.Velocity.Y);
            if (ball.Position.Y < brick.Bounds.MidY)
                ball.Position = new SKPoint(ball.Position.X, brick.Bounds.Top - ball.Radius - 1);
            else
                ball.Position = new SKPoint(ball.Position.X, brick.Bounds.Bottom + ball.Radius + 1);
        }

        // Hit brick
        HitBrick(brick);

        return true;
    }

    /// <summary>
    /// Deteksi tabrakan laser dengan brick.
    /// </summary>
    private bool CheckLaserBrick(Laser laser, Brick brick)
    {
        if (!CheckRectCollision(laser.Bounds, brick.Bounds))
            return false;

        HitBrick(brick);
        return true;
    }

    /// <summary>
    /// Proses pukulan ke brick.
    /// </summary>
    private void HitBrick(Brick brick)
    {
        brick.Hit();

        if (brick.IsDestroyed)
        {
            // Tambah skor
            int points = ScoreManager.PointsForBrick(brick.BrickType);
            _engine.ScoreManager.AddScore(points);

            SKPoint mid = new SKPoint(brick.Bounds.MidX, brick.Bounds.MidY);

            // Efek partikel
            _engine.ParticleSystem.CreateBrickDestroyEffect(mid, brick.Color);

            // Brick explosive
            if (brick.BrickType == BrickType.Explosive)
            {
                ExplodeNearbyBricks(brick);
            }

            // Brick power-up
            if (brick.BrickType == BrickType.PowerUp)
            {
                SpawnPowerUp(mid);
            }
        }
        else
        {
            // Multi-hit brick: efek partikel kecil
            SKPoint mid = new SKPoint(brick.Bounds.MidX, brick.Bounds.MidY);
            _engine.ParticleSystem.CreateBrickHitEffect(mid, brick.Color);
        }
    }

    /// <summary>
    /// Meledakkan brick di sekitar.
    /// </summary>
    private void ExplodeNearbyBricks(Brick center)
    {
        float radius = 60;
        SKPoint centerMid = new SKPoint(center.Bounds.MidX, center.Bounds.MidY);

        foreach (var brick in _engine.Bricks.Where(b => !b.IsDestroyed))
        {
            SKPoint brickMid = new SKPoint(brick.Bounds.MidX, brick.Bounds.MidY);
            float dist = SKPoint.Distance(centerMid, brickMid);
            if (dist < radius && brick != center)
            {
                brick.Destroy();
                int points = ScoreManager.PointsForBrick(brick.BrickType);
                _engine.ScoreManager.AddScore(points);
                _engine.ParticleSystem.CreateBrickDestroyEffect(brickMid, brick.Color);
            }
        }
        _engine.ParticleSystem.CreateExplosionEffect(centerMid);
    }

    /// <summary>
    /// Spawn power-up di posisi tertentu.
    /// </summary>
    private void SpawnPowerUp(SKPoint position)
    {
        var types = Enum.GetValues<PowerUpType>();
        var type = types[_random.Next(types.Length)];
        _engine.PowerUps.Add(new PowerUp(position.X, position.Y, type));
    }

    /// <summary>
    /// Deteksi tabrakan ball dengan power-up.
    /// </summary>
    private bool CheckBallPowerUp(Ball ball, PowerUp pu)
    {
        return CheckCircleRectCollision(ball.Position, ball.Radius, pu.Bounds);
    }

    // --- Utility collision methods ---

    private bool CheckCircleRectCollision(SKPoint circlePos, float radius, SKRect rect)
    {
        float closestX = Math.Clamp(circlePos.X, rect.Left, rect.Right);
        float closestY = Math.Clamp(circlePos.Y, rect.Top, rect.Bottom);
        float dx = circlePos.X - closestX;
        float dy = circlePos.Y - closestY;
        return (dx * dx + dy * dy) < (radius * radius);
    }

    private bool CheckRectCollision(SKRect a, SKRect b)
    {
        return a.IntersectsWith(b);
    }

    private SKPoint GetOverlap(SKPoint circlePos, float radius, SKRect rect)
    {
        float closestX = Math.Clamp(circlePos.X, rect.Left, rect.Right);
        float closestY = Math.Clamp(circlePos.Y, rect.Top, rect.Bottom);
        float dx = circlePos.X - closestX;
        float dy = circlePos.Y - closestY;
        return new SKPoint(MathF.Abs(dx), MathF.Abs(dy));
    }
}
