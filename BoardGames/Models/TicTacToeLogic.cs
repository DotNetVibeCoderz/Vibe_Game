using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGames.Models
{
    public class TicTacToeLogic
    {
        public CellState[] Board { get; private set; }
        public CellState CurrentTurn { get; private set; }
        public CellState Winner { get; private set; }
        public bool IsDraw { get; private set; }
        public bool IsGameOver { get; private set; }

        public TicTacToeLogic()
        {
            NewGame();
        }

        public void NewGame()
        {
            Board = new CellState[9];
            for (int i = 0; i < 9; i++) Board[i] = CellState.Empty;
            CurrentTurn = CellState.Player1; // X starts
            Winner = CellState.Empty;
            IsDraw = false;
            IsGameOver = false;
        }

        public bool MakeMove(int index)
        {
            if (IsGameOver || index < 0 || index >= 9 || Board[index] != CellState.Empty)
                return false;

            Board[index] = CurrentTurn;
            CheckGameState();
            if (!IsGameOver)
            {
                CurrentTurn = (CurrentTurn == CellState.Player1) ? CellState.Player2 : CellState.Player1;
            }
            return true;
        }

        private void CheckGameState()
        {
            int[][] wins = new int[][]
            {
                new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8}, // Rows
                new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8}, // Cols
                new[] {0,4,8}, new[] {2,4,6}                 // Diags
            };

            foreach (var line in wins)
            {
                if (Board[line[0]] != CellState.Empty &&
                    Board[line[0]] == Board[line[1]] &&
                    Board[line[1]] == Board[line[2]])
                {
                    Winner = Board[line[0]];
                    IsGameOver = true;
                    return;
                }
            }

            if (Board.All(c => c != CellState.Empty))
            {
                IsDraw = true;
                IsGameOver = true;
            }
        }

        public int GetBestMove(Difficulty difficulty)
        {
            if (difficulty == Difficulty.Beginner)
            {
                // Random move
                var available = Board.Select((c, i) => new { c, i }).Where(x => x.c == CellState.Empty).Select(x => x.i).ToList();
                if (available.Count == 0) return -1;
                return available[new Random().Next(available.Count)];
            }
            
            // Minimax for Intermediate/Expert
            int bestVal = -1000;
            int bestMove = -1;
            
            // If Intermediate, sometimes allow random error (20% chance)
            if (difficulty == Difficulty.Intermediate && new Random().NextDouble() > 0.8)
            {
                 var available = Board.Select((c, i) => new { c, i }).Where(x => x.c == CellState.Empty).Select(x => x.i).ToList();
                 return available[new Random().Next(available.Count)];
            }

            for (int i = 0; i < 9; i++)
            {
                if (Board[i] == CellState.Empty)
                {
                    Board[i] = CurrentTurn;
                    int moveVal = Minimax(Board, 0, false, CurrentTurn);
                    Board[i] = CellState.Empty;

                    if (moveVal > bestVal)
                    {
                        bestMove = i;
                        bestVal = moveVal;
                    }
                }
            }
            return bestMove;
        }

        private int Minimax(CellState[] board, int depth, bool isMax, CellState player)
        {
            CellState opponent = (player == CellState.Player1) ? CellState.Player2 : CellState.Player1;
            
            int score = Evaluate();
            if (score == 10) return score - depth;
            if (score == -10) return score + depth;
            if (!board.Any(c => c == CellState.Empty)) return 0;

            if (isMax)
            {
                int best = -1000;
                for (int i = 0; i < 9; i++)
                {
                    if (board[i] == CellState.Empty)
                    {
                        board[i] = player;
                        best = Math.Max(best, Minimax(board, depth + 1, !isMax, player));
                        board[i] = CellState.Empty;
                    }
                }
                return best;
            }
            else
            {
                int best = 1000;
                for (int i = 0; i < 9; i++)
                {
                    if (board[i] == CellState.Empty)
                    {
                        board[i] = opponent;
                        best = Math.Min(best, Minimax(board, depth + 1, !isMax, player));
                        board[i] = CellState.Empty;
                    }
                }
                return best;
            }
        }

        private int Evaluate()
        {
            int[][] wins = new int[][]
            {
                new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
                new[] {0,3,6}, new[] {1,4,7}, new[] {2,5,8},
                new[] {0,4,8}, new[] {2,4,6}
            };

            foreach (var line in wins)
            {
                if (Board[line[0]] == Board[line[1]] && Board[line[1]] == Board[line[2]])
                {
                    if (Board[line[0]] == CurrentTurn) return 10;
                    if (Board[line[0]] != CellState.Empty) return -10;
                }
            }
            return 0;
        }
    }
}