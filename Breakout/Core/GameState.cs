namespace Breakout.Core;

/// <summary>
/// Enum yang merepresentasikan state / kondisi game saat ini.
/// </summary>
public enum GameState
{
    Menu,       // Menu utama
    Playing,    // Sedang bermain
    Paused,     // Di-pause
    GameOver,   // Game over (kehabisan nyawa)
    LevelClear, // Level berhasil diselesaikan
    Settings,   // Menu settings
    About,      // Menu about
    LevelEditor // Level editor
}
