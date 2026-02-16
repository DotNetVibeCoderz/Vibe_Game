using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using CrossWordsNet.Models;
using CrossWordsNet.Services;

namespace CrossWordsNet.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel _mainViewModel;
        private ObservableCollection<CrosswordCell> _gridCells;
        private ObservableCollection<CrosswordWord> _acrossClues;
        private ObservableCollection<CrosswordWord> _downClues;
        private string _timerString;
        private int _score;
        private IDisposable _timerSubscription;
        private TimeSpan _elapsed;
        private CrosswordCell _selectedCell;

        public GameViewModel(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            
            StartNewGame();
            
            BackCommand = ReactiveCommand.Create(GoBack);
            CheckCommand = ReactiveCommand.Create(CheckSolution);
            HintCommand = ReactiveCommand.Create(UseHint);
        }

        public ObservableCollection<CrosswordCell> GridCells
        {
            get => _gridCells;
            set => this.RaiseAndSetIfChanged(ref _gridCells, value);
        }

        public ObservableCollection<CrosswordWord> AcrossClues
        {
            get => _acrossClues;
            set => this.RaiseAndSetIfChanged(ref _acrossClues, value);
        }

        public ObservableCollection<CrosswordWord> DownClues
        {
            get => _downClues;
            set => this.RaiseAndSetIfChanged(ref _downClues, value);
        }

        public string TimerString
        {
            get => _timerString;
            set => this.RaiseAndSetIfChanged(ref _timerString, value);
        }

        public int Score
        {
            get => _score;
            set => this.RaiseAndSetIfChanged(ref _score, value);
        }

        public CrosswordCell SelectedCell
        {
            get => _selectedCell;
            set 
            {
                if (_selectedCell != value)
                {
                     if (_selectedCell != null) _selectedCell.IsHighlighted = false;
                     this.RaiseAndSetIfChanged(ref _selectedCell, value);
                     if (_selectedCell != null) 
                     {
                         _selectedCell.IsHighlighted = true;
                         SoundManager.PlayClick();
                     }
                }
            }
        }

        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        public ReactiveCommand<Unit, Unit> CheckCommand { get; }
        public ReactiveCommand<Unit, Unit> HintCommand { get; }

        private void StartNewGame()
        {
            var generator = new CrosswordGenerator();
            var (cells, words) = generator.GenerateLevel();

            GridCells = new ObservableCollection<CrosswordCell>(cells);
            AcrossClues = new ObservableCollection<CrosswordWord>(words.Where(w => w.Direction == Direction.Horizontal).OrderBy(w => w.Number));
            DownClues = new ObservableCollection<CrosswordWord>(words.Where(w => w.Direction == Direction.Vertical).OrderBy(w => w.Number));

            _elapsed = TimeSpan.Zero;
            _timerSubscription = Observable.Interval(TimeSpan.FromSeconds(1))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => 
                {
                    _elapsed = _elapsed.Add(TimeSpan.FromSeconds(1));
                    TimerString = _elapsed.ToString(@"mm\:ss");
                });
            
            Score = 0;
            TimerString = "00:00";
            
            SelectedCell = GridCells.FirstOrDefault(c => !c.IsBlock);
        }

        private void GoBack()
        {
            SoundManager.PlayClick();
            _timerSubscription?.Dispose();
            _mainViewModel.GoToMenu();
        }

        private void CheckSolution()
        {
            bool allCorrect = true;
            bool anyWrong = false;

            foreach(var cell in GridCells)
            {
                if (!cell.IsBlock)
                {
                    if (string.IsNullOrEmpty(cell.UserInput) || 
                        cell.UserInput.ToUpper() != cell.Letter.ToString().ToUpper())
                    {
                        allCorrect = false;
                        if (!string.IsNullOrEmpty(cell.UserInput)) anyWrong = true;
                    }
                }
            }

            if (allCorrect)
            {
                Score += 100; // Bonus for completion
                SoundManager.PlayWin();
            }
            else
            {
                if (anyWrong)
                    SoundManager.PlayFail();
                else
                    SoundManager.PlayClick(); // Just incomplete
            }
        }

        private void UseHint()
        {
            if (SelectedCell != null && !SelectedCell.IsBlock)
            {
                if (SelectedCell.UserInput != SelectedCell.Letter.ToString())
                {
                    SelectedCell.UserInput = SelectedCell.Letter.ToString();
                    Score -= 10; // Penalty
                    SoundManager.PlayClick();
                }
            }
        }

        public void MoveSelection(int rowDelta, int colDelta)
        {
            if (SelectedCell == null) return;

            int newRow = SelectedCell.Row + rowDelta;
            int newCol = SelectedCell.Col + colDelta;

            var target = GridCells.FirstOrDefault(c => c.Row == newRow && c.Col == newCol);
            if (target != null && !target.IsBlock)
            {
                SelectedCell = target;
            }
        }

        public void InputLetter(string letter)
        {
            if (SelectedCell != null && !SelectedCell.IsBlock && !string.IsNullOrEmpty(letter))
            {
                SelectedCell.UserInput = letter.ToUpper();
                SoundManager.PlayClick();
            }
        }
    }
}