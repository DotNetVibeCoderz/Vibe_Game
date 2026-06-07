using Breakout.Core;
using Breakout.GameObjects;
using SkiaSharp;

namespace Breakout.Levels;

/// <summary>
/// Editor level visual sederhana untuk mendesain layout brick.
/// </summary>
public class LevelEditor
{
    public bool IsActive { get; set; }
    public int GridCols { get; set; } = 10;
    public int GridRows { get; set; } = 6;
    public float CellWidth { get; set; } = 60;
    public float CellHeight { get; set; } = 25;
    public float CellMargin { get; set; } = 4;

    private readonly List<EditorCell> _cells = new();
    private BrickType _selectedType = BrickType.Standard;
    private int _selectedRow = -1;
    private int _selectedCol = -1;
    private float _gridStartX = 40;
    private float _gridStartY = 80;
    private SKPoint _lastMousePos;

    public LevelEditor()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        _cells.Clear();
        float totalWidth = GridCols * CellWidth + (GridCols - 1) * CellMargin;
        _gridStartX = (800 - totalWidth) / 2;

        for (int row = 0; row < GridRows; row++)
        {
            for (int col = 0; col < GridCols; col++)
            {
                _cells.Add(new EditorCell
                {
                    Row = row,
                    Col = col,
                    Type = BrickType.Standard,
                    IsEmpty = true
                });
            }
        }
    }

    public void HandleMouseClick(SKPoint mousePos)
    {
        if (!IsActive) return;

        var (row, col) = GetCellAt(mousePos);
        if (row >= 0 && col >= 0)
        {
            var cell = _cells.FirstOrDefault(c => c.Row == row && c.Col == col);
            if (cell != null)
            {
                if (cell.IsEmpty)
                {
                    cell.Type = _selectedType;
                    cell.IsEmpty = false;
                }
                else if (cell.Type == _selectedType)
                {
                    cell.IsEmpty = true;
                }
                else
                {
                    cell.Type = _selectedType;
                }
            }
        }
    }

    /// <summary>
    /// Konversi grid editor ke BrickConfig list.
    /// </summary>
    public List<BrickConfig> ExportLevel()
    {
        var bricks = new List<BrickConfig>();
        float totalWidth = GridCols * CellWidth + (GridCols - 1) * CellMargin;
        float startX = (800 - totalWidth) / 2;

        foreach (var cell in _cells.Where(c => !c.IsEmpty))
        {
            float x = startX + cell.Col * (CellWidth + CellMargin);
            float y = _gridStartY + cell.Row * (CellHeight + CellMargin);

            bricks.Add(new BrickConfig
            {
                X = x,
                Y = y,
                Width = CellWidth,
                Height = CellHeight,
                Row = cell.Row,
                Column = cell.Col,
                Type = cell.Type
            });
        }

        return bricks;
    }

    private (int row, int col) GetCellAt(SKPoint pos)
    {
        float totalWidth = GridCols * CellWidth + (GridCols - 1) * CellMargin;
        float startX = (800 - totalWidth) / 2;

        for (int row = 0; row < GridRows; row++)
        {
            for (int col = 0; col < GridCols; col++)
            {
                float x = startX + col * (CellWidth + CellMargin);
                float y = _gridStartY + row * (CellHeight + CellMargin);
                var rect = new SKRect(x, y, x + CellWidth, y + CellHeight);

                if (rect.Contains(pos))
                    return (row, col);
            }
        }

        return (-1, -1);
    }

    public void Render(SKCanvas canvas)
    {
        if (!IsActive) return;

        // Background semi-transparan
        using var bgPaint = new SKPaint
        {
            Color = new SKColor(0, 0, 0, 200)
        };
        canvas.DrawRect(0, 0, 800, 600, bgPaint);

        // Grid
        float totalWidth = GridCols * CellWidth + (GridCols - 1) * CellMargin;
        float startX = (800 - totalWidth) / 2;

        // Grid lines
        using var gridPaint = new SKPaint
        {
            Color = new SKColor(100, 100, 100, 100),
            StrokeWidth = 0.5f,
            Style = SKPaintStyle.Stroke
        };

        for (int row = 0; row <= GridRows; row++)
        {
            float y = _gridStartY + row * (CellHeight + CellMargin);
            canvas.DrawLine(startX, y, startX + totalWidth, y, gridPaint);
        }

        for (int col = 0; col <= GridCols; col++)
        {
            float x = startX + col * (CellWidth + CellMargin);
            canvas.DrawLine(x, _gridStartY, x, _gridStartY + GridRows * (CellHeight + CellMargin), gridPaint);
        }

        // Cells
        foreach (var cell in _cells)
        {
            float x = startX + cell.Col * (CellWidth + CellMargin);
            float y = _gridStartY + cell.Row * (CellHeight + CellMargin);
            var rect = new SKRect(x, y, x + CellWidth, y + CellHeight);

            var color = cell.IsEmpty ? new SKColor(50, 50, 50, 100) : GetBrickColor(cell.Type);

            using var cellPaint = new SKPaint
            {
                Color = color,
                IsAntialias = true
            };
            canvas.DrawRoundRect(rect, 3, 3, cellPaint);

            if (!cell.IsEmpty)
            {
                using var borderPaint = new SKPaint
                {
                    Color = color.WithAlpha(200),
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 1
                };
                canvas.DrawRoundRect(rect, 3, 3, borderPaint);
            }
        }

        // Toolbar
        using var toolbarPaint = new SKPaint
        {
            Color = new SKColor(30, 30, 50, 200)
        };
        canvas.DrawRect(0, 0, 800, 40, toolbarPaint);

        // Toolbar text
        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 14,
            IsAntialias = true
        };
        canvas.DrawText("Level Editor - Click to place/remove bricks | [1]Std [2]Multi [3]Explosive [4]PowerUp [S]ave [ESC] Exit",
            10, 25, textPaint);

        // Selected type indicator
        using var selPaint = new SKPaint
        {
            Color = GetBrickColor(_selectedType),
            IsAntialias = true
        };
        canvas.DrawCircle(750, 20, 8, selPaint);
    }

    private SKColor GetBrickColor(BrickType type) => type switch
    {
        BrickType.Standard => new SKColor(255, 50, 50),
        BrickType.MultiHit => new SKColor(100, 100, 255),
        BrickType.Explosive => new SKColor(255, 100, 0),
        BrickType.PowerUp => new SKColor(0, 255, 150),
        _ => new SKColor(100, 100, 100)
    };

    public void SelectType(BrickType type) => _selectedType = type;

    private class EditorCell
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public BrickType Type { get; set; }
        public bool IsEmpty { get; set; } = true;
    }
}
