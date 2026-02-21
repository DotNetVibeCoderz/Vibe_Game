namespace DoomNet
{
    /// <summary>
    /// Layanan untuk menangani logika game sisi C#
    /// Bisa digunakan untuk menyimpan data persisten, high scores, atau konfigurasi
    /// </summary>
    public class GameService
    {
        public int HighScore { get; private set; } = 0;
        public PlayerSettings Settings { get; set; } = new PlayerSettings();

        public void AddScore(int points)
        {
            if (points > HighScore)
            {
                HighScore = points;
                // TODO: Simpan ke storage
            }
        }

        public void SaveSettings(PlayerSettings settings)
        {
            Settings = settings;
            // TODO: Simpan ke file config
        }
    }

    public class PlayerSettings
    {
        public float Sensitivity { get; set; } = 5.0f;
        public int Volume { get; set; } = 70;
        public bool ShowFPS { get; set; } = true;
    }
}