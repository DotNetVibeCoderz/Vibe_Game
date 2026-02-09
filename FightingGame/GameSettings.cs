namespace FightingGame
{
    public enum DifficultyLevel
    {
        Easy,
        Normal,
        Hard
    }

    public static class GameSettings
    {
        public static DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Normal;
        public static bool SoundEnabled { get; set; } = true;
    }
}