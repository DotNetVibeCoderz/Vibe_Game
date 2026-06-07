namespace Breakout.Core;

/// <summary>
/// Mengelola input keyboard & mouse untuk game.
/// </summary>
public class InputManager
{
    private readonly HashSet<Keys> _keysDown = new();
    private readonly HashSet<Keys> _keysPressed = new(); // tombol baru ditekan
    private readonly HashSet<Keys> _keysReleased = new(); // tombol baru dilepas

    private Keys _lastKeyPressed;

    public PointF MousePosition { get; private set; }
    public bool MouseLeftDown { get; private set; }
    public bool MouseLeftClicked { get; private set; }

    public void KeyDown(Keys key)
    {
        if (!_keysDown.Contains(key))
        {
            _keysPressed.Add(key);
            _lastKeyPressed = key;
        }
        _keysDown.Add(key);
    }

    public void KeyUp(Keys key)
    {
        _keysDown.Remove(key);
        _keysReleased.Add(key);
    }

    public void SetMousePosition(float x, float y)
    {
        MousePosition = new PointF(x, y);
    }

    public void SetMouseDown(bool down)
    {
        if (down && !MouseLeftDown)
            MouseLeftClicked = true;
        MouseLeftDown = down;
    }

    /// <summary>
    /// Cek apakah suatu tombol sedang ditekan.
    /// </summary>
    public bool IsKeyDown(Keys key) => _keysDown.Contains(key);

    /// <summary>
    /// Cek apakah suatu tombol baru saja ditekan (once per press).
    /// </summary>
    public bool IsKeyPressed(Keys key) => _keysPressed.Contains(key);

    /// <summary>
    /// Cek apakah suatu tombol baru saja dilepas.
    /// </summary>
    public bool IsKeyReleased(Keys key) => _keysReleased.Contains(key);

    /// <summary>
    /// Dapatkan arah paddle berdasarkan input keyboard.
    /// </summary>
    public float GetPaddleDirection()
    {
        float dir = 0;
        if (IsKeyDown(Keys.Left) || IsKeyDown(Keys.A)) dir -= 1;
        if (IsKeyDown(Keys.Right) || IsKeyDown(Keys.D)) dir += 1;
        return dir;
    }

    /// <summary>
    /// Panggil setiap akhir frame untuk membersihkan one-shot events.
    /// </summary>
    public void EndFrame()
    {
        _keysPressed.Clear();
        _keysReleased.Clear();
        MouseLeftClicked = false;
    }

    public Keys LastKeyPressed => _lastKeyPressed;
}
