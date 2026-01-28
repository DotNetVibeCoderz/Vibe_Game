namespace BoardGames.Models
{
    public enum GameType
    {
        None,
        Othello,
        Checkers,
        TicTacToe
    }

    public enum PlayerType
    {
        Human,
        AI
    }

    public enum Difficulty
    {
        Beginner,
        Intermediate,
        Expert
    }

    public enum CellState
    {
        Empty,
        Player1, // Usually Black or X
        Player2  // Usually White or O
    }

    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }
}