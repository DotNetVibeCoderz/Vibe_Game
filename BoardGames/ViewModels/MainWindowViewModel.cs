using Avalonia.Threading;
using BoardGames.Models;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;

namespace BoardGames.ViewModels
{
    public class MainWindowViewModel : ReactiveObject
    {
        // Navigation State
        private bool _isMenuVisible = true;
        public bool IsMenuVisible { get => _isMenuVisible; set => this.RaiseAndSetIfChanged(ref _isMenuVisible, value); }
        
        private bool _isGameVisible = false;
        public bool IsGameVisible { get => _isGameVisible; set => this.RaiseAndSetIfChanged(ref _isGameVisible, value); }

        // Settings
        private GameType _selectedGame = GameType.None;
        public GameType SelectedGame { get => _selectedGame; set => this.RaiseAndSetIfChanged(ref _selectedGame, value); }

        private PlayerType _opponent = PlayerType.AI;
        public PlayerType Opponent { get => _opponent; set => this.RaiseAndSetIfChanged(ref _opponent, value); }

        private Difficulty _difficulty = Difficulty.Beginner;
        public Difficulty Difficulty { get => _difficulty; set => this.RaiseAndSetIfChanged(ref _difficulty, value); }

        // Game State
        private string _title = "Board Games Collection";
        public string Title { get => _title; set => this.RaiseAndSetIfChanged(ref _title, value); }

        private string _statusMessage = "Welcome";
        public string StatusMessage { get => _statusMessage; set => this.RaiseAndSetIfChanged(ref _statusMessage, value); }

        private string _timerText = "00:00";
        public string TimerText { get => _timerText; set => this.RaiseAndSetIfChanged(ref _timerText, value); }

        private int _score1;
        public int Score1 { get => _score1; set => this.RaiseAndSetIfChanged(ref _score1, value); }

        private int _score2;
        public int Score2 { get => _score2; set => this.RaiseAndSetIfChanged(ref _score2, value); }

        // Boards
        public ObservableCollection<CellViewModel> BoardCells { get; } = new ObservableCollection<CellViewModel>();
        // Using a 1D collection for simplicity in ItemsControl, mapped from 2D logic
        
        public int Columns { get; set; } // For UniformGrid

        // Logic Instances
        private TicTacToeLogic _tictactoe;
        private OthelloLogic _othello;
        private CheckersLogic _checkers;
        private DispatcherTimer _timer;
        private DateTime _startTime;

        // Commands
        public ReactiveCommand<string, Unit> SelectGameCommand { get; }
        public ReactiveCommand<Unit, Unit> BackToMenuCommand { get; }
        public ReactiveCommand<CellViewModel, Unit> CellClickedCommand { get; }
        public ReactiveCommand<string, Unit> ChangeDifficultyCommand { get; }

        public MainWindowViewModel()
        {
            SelectGameCommand = ReactiveCommand.Create<string>(StartGame);
            BackToMenuCommand = ReactiveCommand.Create(ShowMenu);
            CellClickedCommand = ReactiveCommand.Create<CellViewModel>(OnCellClicked);
            ChangeDifficultyCommand = ReactiveCommand.Create<string>(SetDifficulty);

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => TimerText = (DateTime.Now - _startTime).ToString(@"mm\:ss");
        }

        private void SetDifficulty(string level)
        {
            if (Enum.TryParse(level, out Difficulty d)) Difficulty = d;
        }

        private void ShowMenu()
        {
            _timer.Stop();
            IsMenuVisible = true;
            IsGameVisible = false;
            Title = "Board Games Collection";
            BoardCells.Clear();
        }

        private void StartGame(string gameName)
        {
            if (!Enum.TryParse(gameName, out GameType type)) return;
            SelectedGame = type;
            Title = gameName;
            IsMenuVisible = false;
            IsGameVisible = true;
            _startTime = DateTime.Now;
            _timer.Start();
            
            Score1 = 0; Score2 = 0;
            StatusMessage = "Player 1's Turn";

            SetupBoard();
        }

        private void SetupBoard()
        {
            BoardCells.Clear();
            switch (SelectedGame)
            {
                case GameType.TicTacToe:
                    Columns = 3;
                    _tictactoe = new TicTacToeLogic();
                    for(int i=0; i<9; i++) BoardCells.Add(new CellViewModel(i, 0, 0));
                    this.RaisePropertyChanged(nameof(Columns));
                    break;
                case GameType.Othello:
                    Columns = 8;
                    _othello = new OthelloLogic();
                    for(int i=0; i<64; i++) BoardCells.Add(new CellViewModel(i, i/8, i%8));
                    this.RaisePropertyChanged(nameof(Columns));
                    SyncOthelloBoard();
                    break;
                case GameType.Checkers:
                    Columns = 8;
                    _checkers = new CheckersLogic();
                    for(int i=0; i<64; i++) BoardCells.Add(new CellViewModel(i, i/8, i%8));
                    this.RaisePropertyChanged(nameof(Columns));
                    SyncCheckersBoard();
                    break;
            }
        }

        private void OnCellClicked(CellViewModel cell)
        {
            // Human Move
            bool moveMade = false;

            if (SelectedGame == GameType.TicTacToe)
            {
                if (_tictactoe.MakeMove(cell.Id))
                {
                    cell.Text = _tictactoe.Board[cell.Id] == CellState.Player1 ? "X" : "O";
                    moveMade = true;
                    if (_tictactoe.IsGameOver) EndGame(_tictactoe.Winner);
                }
            }
            else if (SelectedGame == GameType.Othello)
            {
                if (_othello.MakeMove(cell.Row, cell.Col))
                {
                    SyncOthelloBoard();
                    moveMade = true;
                    if (_othello.IsGameOver) EndGame(_othello.BlackScore > _othello.WhiteScore ? CellState.Player1 : CellState.Player2);
                }
            }
            else if (SelectedGame == GameType.Checkers)
            {
                // Simplified Click Handling: Select Source, Then Dest
                // This requires state in VM for "SelectedSource"
                // For brevity, let's assume this part is tricky without more code. 
                // I will implement a basic "First Click Select, Second Click Move" logic.
                HandleCheckersClick(cell);
                // Return here because checkers logic is internal to that method
                return; 
            }

            if (moveMade && Opponent == PlayerType.AI && !_isGameOver())
            {
                // AI Move (Delayed slightly for UX)
                Dispatcher.UIThread.Post(async () => {
                    await System.Threading.Tasks.Task.Delay(500);
                    DoAIMove();
                });
            }
        }

        private CellViewModel _selectedChecker;
        private void HandleCheckersClick(CellViewModel cell)
        {
             if (_selectedChecker == null)
             {
                 // Try select
                 if (_checkers.Board[cell.Row, cell.Col].Player == _checkers.CurrentTurn)
                 {
                     _selectedChecker = cell;
                     StatusMessage = $"Selected {cell.Row},{cell.Col}. Choose destination.";
                 }
             }
             else
             {
                 // Try move
                 if (_checkers.MakeMove(_selectedChecker.Row, _selectedChecker.Col, cell.Row, cell.Col))
                 {
                     SyncCheckersBoard();
                     _selectedChecker = null;
                     if (_checkers.IsGameOver) EndGame(_checkers.Winner);
                     else
                     {
                         if(Opponent == PlayerType.AI) DoAIMove();
                     }
                 }
                 else
                 {
                     _selectedChecker = null; // Cancel selection
                     StatusMessage = "Invalid Move. Select piece again.";
                 }
             }
        }

        private void DoAIMove()
        {
            if (SelectedGame == GameType.TicTacToe)
            {
                int move = _tictactoe.GetBestMove(Difficulty);
                if (move != -1)
                {
                    _tictactoe.MakeMove(move);
                    BoardCells[move].Text = "O";
                    if (_tictactoe.IsGameOver) EndGame(_tictactoe.Winner);
                }
            }
            else if (SelectedGame == GameType.Othello)
            {
                var move = _othello.GetBestMove(Difficulty);
                if (move != null)
                {
                    _othello.MakeMove(move.Item1, move.Item2);
                    SyncOthelloBoard();
                    if (_othello.IsGameOver) EndGame(_othello.BlackScore > _othello.WhiteScore ? CellState.Player1 : CellState.Player2);
                }
            }
            else if (SelectedGame == GameType.Checkers)
            {
                 var move = _checkers.GetBestMove(Difficulty);
                 if (move != null)
                 {
                     _checkers.MakeMove(move.FromR, move.FromC, move.ToR, move.ToC);
                     SyncCheckersBoard();
                     if (_checkers.IsGameOver) EndGame(_checkers.Winner);
                 }
            }
        }

        private bool _isGameOver()
        {
            if (SelectedGame == GameType.TicTacToe) return _tictactoe.IsGameOver;
            if (SelectedGame == GameType.Othello) return _othello.IsGameOver;
            if (SelectedGame == GameType.Checkers) return _checkers.IsGameOver;
            return false;
        }

        private void EndGame(CellState winner)
        {
            _timer.Stop();
            string w = winner == CellState.Player1 ? "Player 1" : "Player 2";
            if (winner == CellState.Empty) w = "Draw";
            StatusMessage = $"Game Over! Winner: {w}";
        }

        private void SyncOthelloBoard()
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var cell = BoardCells[r * 8 + c];
                    var state = _othello.Board[r, c];
                    cell.Color = state == CellState.Empty ? "Transparent" : (state == CellState.Player1 ? "Black" : "White");
                    cell.Text = "";
                }
            Score1 = _othello.BlackScore;
            Score2 = _othello.WhiteScore;
            StatusMessage = $"Turn: {(_othello.CurrentTurn == CellState.Player1 ? "Black" : "White")}";
        }

        private void SyncCheckersBoard()
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    var cell = BoardCells[r * 8 + c];
                    var piece = _checkers.Board[r, c];
                    if (piece.Player == CellState.Empty) 
                    {
                        cell.Text = "";
                        cell.Color = "Transparent";
                    }
                    else
                    {
                        cell.Color = piece.Player == CellState.Player1 ? "Red" : "White"; // Red vs White usually
                        cell.Text = piece.Type == CheckersLogic.PieceType.King ? "K" : "";
                    }
                    // Checkerboard background handled in View usually, but here we set piece color
                }
             StatusMessage = $"Turn: {(_checkers.CurrentTurn == CellState.Player1 ? "Red" : "White")}";
        }
    }

    public class CellViewModel : ReactiveObject
    {
        public int Id { get; }
        public int Row { get; }
        public int Col { get; }

        private string _text = "";
        public string Text { get => _text; set => this.RaiseAndSetIfChanged(ref _text, value); }

        private string _color = "Transparent";
        public string Color { get => _color; set => this.RaiseAndSetIfChanged(ref _color, value); }

        public CellViewModel(int id, int r, int c)
        {
            Id = id; Row = r; Col = c;
        }
    }
}