using Avalonia.Threading;
using ReactiveUI;
using SudokuNet.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Timers;

namespace SudokuNet.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private ObservableCollection<SudokuCell> _cells = null!;
        private string _timerText = "00:00";
        private int _mistakes;
        private bool _isGameWon;
        private System.Timers.Timer _timer = null!;
        private DateTime _startTime;
        private int[,] _solution = null!;
        private SudokuCell? _selectedCell;

        public ObservableCollection<SudokuCell> Cells
        {
            get => _cells;
            set
            {
                if (_cells != value)
                {
                    _cells = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TimerText
        {
            get => _timerText;
            set
            {
                if (_timerText != value)
                {
                    _timerText = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Mistakes
        {
            get => _mistakes;
            set
            {
                if (_mistakes != value)
                {
                    _mistakes = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsGameWon
        {
            get => _isGameWon;
            set
            {
                if (_isGameWon != value)
                {
                    _isGameWon = value;
                    OnPropertyChanged();
                }
            }
        }

        public SudokuCell? SelectedCell
        {
            get => _selectedCell;
            set
            {
                if (_selectedCell != null) _selectedCell.IsSelected = false;
                _selectedCell = value;
                if (_selectedCell != null) _selectedCell.IsSelected = true;
                OnPropertyChanged();
            }
        }

        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        public ReactiveCommand<string, Unit> InputCommand { get; }
        public ReactiveCommand<SudokuCell, Unit> SelectCellCommand { get; }
        public ReactiveCommand<Unit, Unit> SolveCommand { get; }

        public GameViewModel(string difficulty, Action backAction)
        {
            BackCommand = ReactiveCommand.Create(() => 
            {
                if (_timer != null) _timer.Stop();
                backAction();
            });
            InputCommand = ReactiveCommand.Create<string>(InputNumber);
            SelectCellCommand = ReactiveCommand.Create<SudokuCell>(cell => SelectedCell = cell);
            SolveCommand = ReactiveCommand.Create(SolveGame);
            
            StartGame(difficulty);
        }

        private void StartGame(string difficulty)
        {
            if (_timer != null) _timer.Stop();
            
            _solution = SudokuGenerator.GenerateSolution();
            int removeCount = difficulty switch
            {
                "Easy" => 30,
                "Medium" => 40,
                "Hard" => 50,
                "Expert" => 55,
                "Extreme" => 60,
                _ => 30
            };

            var puzzle = SudokuGenerator.RemoveDigits(_solution, removeCount);
            var cells = new List<SudokuCell>();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var val = puzzle[i, j];
                    cells.Add(new SudokuCell
                    {
                        Row = i,
                        Col = j,
                        Value = val,
                        IsFixed = val != 0,
                        IsValid = true
                    });
                }
            }
            Cells = new ObservableCollection<SudokuCell>(cells);

            Mistakes = 0;
            IsGameWon = false;
            _startTime = DateTime.Now;
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var span = DateTime.Now - _startTime;
            Dispatcher.UIThread.Post(() =>
            {
                TimerText = span.ToString(@"mm\:ss");
            });
        }

        private void InputNumber(string numStr)
        {
            if (SelectedCell == null || SelectedCell.IsFixed) return;

            if (int.TryParse(numStr, out int num))
            {
                SelectedCell.Value = num;
                int correctVal = _solution[SelectedCell.Row, SelectedCell.Col];
                if (num != correctVal)
                {
                    SelectedCell.IsValid = false;
                    Mistakes++;
                }
                else
                {
                    SelectedCell.IsValid = true;
                    CheckWin();
                }
            }
            else if (numStr == "C")
            {
                SelectedCell.Value = 0;
                SelectedCell.IsValid = true;
            }
        }

        private void CheckWin()
        {
            if (Cells.All(c => c.Value != 0))
            {
                bool correct = true;
                foreach(var c in Cells)
                {
                    if (c.Value != _solution[c.Row, c.Col])
                    {
                        correct = false;
                        break;
                    }
                }

                if (correct)
                {
                    IsGameWon = true;
                    if (_timer != null) _timer.Stop();
                }
            }
        }

        private void SolveGame()
        {
            if (_timer != null) _timer.Stop();
            
            foreach (var cell in Cells)
            {
                if (!cell.IsFixed)
                {
                    cell.Value = _solution[cell.Row, cell.Col];
                    cell.IsValid = true;
                }
            }
            IsGameWon = true;
        }
    }
}