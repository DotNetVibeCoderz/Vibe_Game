namespace Breakout.Audio;

/// <summary>
/// Mengelola efek suara dan musik untuk game.
/// Karena kita tidak bisa menyertakan file audio asli,
/// kita akan generate sound effects secara procedural dengan Console.Beep.
/// </summary>
public class AudioManager
{
    public bool SoundEnabled { get; set; } = true;
    public bool MusicEnabled { get; set; } = true;
    public int Volume { get; set; } = 80;

    /// <summary>
    /// Generate suara retro secara procedural (beep).
    /// </summary>
    public void PlayBeepSound(int frequency, int duration)
    {
        if (!SoundEnabled) return;
        try
        {
            Console.Beep(frequency, duration);
        }
        catch
        {
            // Console.Beep mungkin tidak didukung di semua environment
        }
    }

    /// <summary>
    /// Mainkan suara berdasarkan tipe.
    /// </summary>
    public void PlaySoundEffect(SoundType type)
    {
        switch (type)
        {
            case SoundType.BallHit:
                PlayBeepSound(800, 80);
                break;
            case SoundType.BrickHit:
                PlayBeepSound(600, 60);
                break;
            case SoundType.BrickDestroy:
                PlayBeepSound(1000, 100);
                break;
            case SoundType.Explosion:
                PlayBeepSound(200, 200);
                break;
            case SoundType.PowerUpCollect:
                PlayBeepSound(1200, 150);
                break;
            case SoundType.LaserShoot:
                PlayBeepSound(1500, 50);
                break;
            case SoundType.LifeLost:
                PlayBeepSound(300, 300);
                break;
            case SoundType.LevelClear:
                // Play a little melody
                PlayBeepSound(523, 100);
                Thread.Sleep(50);
                PlayBeepSound(659, 100);
                Thread.Sleep(50);
                PlayBeepSound(784, 100);
                Thread.Sleep(50);
                PlayBeepSound(1047, 200);
                break;
            case SoundType.GameOver:
                PlayBeepSound(200, 500);
                break;
            case SoundType.MenuSelect:
                PlayBeepSound(900, 60);
                break;
            case SoundType.MenuNavigate:
                PlayBeepSound(700, 40);
                break;
        }
    }

    /// <summary>
    /// Mainkan efek suara untuk berbagai event game.
    /// </summary>
    public void PlaySound(SoundType type)
    {
        if (!SoundEnabled) return;
        PlaySoundEffect(type);
    }

    /// <summary>
    /// Mainkan musik latar (stub untuk implementasi file .wav).
    /// </summary>
    public void PlayMusic(string trackName)
    {
        if (!MusicEnabled) return;
        // Stub: implementasi nyata akan memutar file .wav atau MIDI
    }

    /// <summary>
    /// Stop musik.
    /// </summary>
    public void StopMusic()
    {
        // Stub
    }
}

/// <summary>
/// Tipe-tipe suara dalam game.
/// </summary>
public enum SoundType
{
    BallHit,
    BrickHit,
    BrickDestroy,
    Explosion,
    PowerUpCollect,
    LaserShoot,
    LifeLost,
    LevelClear,
    GameOver,
    MenuSelect,
    MenuNavigate
}
