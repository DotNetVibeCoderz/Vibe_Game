using SkiaSharp;

namespace Breakout.Effects;

/// <summary>
/// Efek trail (jejak) untuk bola. Menyimpan posisi-posisi terakhir bola
/// dan merendernya dengan efek fade.
/// </summary>
public class BallTrail
{
    private readonly List<TrailPoint> _trailPoints = new();
    private readonly int _maxTrailLength = 15;

    public void Clear() => _trailPoints.Clear();

    /// <summary>
    /// Tambahkan posisi bola ke trail.
    /// </summary>
    public void AddPoint(SKPoint position)
    {
        _trailPoints.Add(new TrailPoint { Position = position, Life = 1.0f });

        if (_trailPoints.Count > _maxTrailLength)
            _trailPoints.RemoveAt(0);
    }

    /// <summary>
    /// Update semua titik trail (fade out).
    /// </summary>
    public void Update(float dt)
    {
        foreach (var tp in _trailPoints)
        {
            tp.Life -= dt * 2.5f;
        }
        _trailPoints.RemoveAll(tp => tp.Life <= 0);
    }

    /// <summary>
    /// Render trail dengan gradien warna.
    /// </summary>
    public void Render(SKCanvas canvas)
    {
        if (_trailPoints.Count < 2) return;

        for (int i = 1; i < _trailPoints.Count; i++)
        {
            var prev = _trailPoints[i - 1];
            var curr = _trailPoints[i];

            if (curr.Life <= 0) continue;

            float alpha = curr.Life * 120;
            float width = curr.Life * 4;

            using var paint = new SKPaint
            {
                Color = new SKColor(0, 200, 255, (byte)alpha),
                IsAntialias = true,
                StrokeWidth = width,
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Stroke
            };

            canvas.DrawLine(prev.Position, curr.Position, paint);
        }
    }

    private class TrailPoint
    {
        public SKPoint Position { get; set; }
        public float Life { get; set; }
    }
}
