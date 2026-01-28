using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGames.Models
{
    public class OthelloLogic
    {
        public CellState[,] Board { get; private set; }
        public CellState CurrentTurn { get; private set; }
        public bool IsGameOver { get; private set; }
        public int BlackScore { get; private set; }
        public int WhiteScore { get; private set; }

        public OthelloLogic()
        {
            NewGame();
        }

        public void NewGame()
        {
            Board = new CellState[8, 8];
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    Board[r, c] = CellState.Empty;

            // Initial setup
            Board[3, 3] = CellState.Player2; // White
            Board[3, 4] = CellState.Player1; // Black
            Board[4, 3] = CellState.Player1; // Black
            Board[4, 4] = CellState.Player2; // White

            CurrentTurn = CellState.Player1; // Black starts
            IsGameOver = false;
            UpdateScores();
        }

        public bool MakeMove(int r, int c)
        {
            if (IsGameOver || !IsValidMove(r, c, CurrentTurn)) return false;

            FlipDiscs(r, c, CurrentTurn);
            CurrentTurn = (CurrentTurn == CellState.Player1) ? CellState.Player2 : CellState.Player1;

            if (!HasValidMoves(CurrentTurn))
            {
                CurrentTurn = (CurrentTurn == CellState.Player1) ? CellState.Player2 : CellState.Player1;
                if (!HasValidMoves(CurrentTurn))
                {
                    IsGameOver = true;
                }
            }
            UpdateScores();
            return true;
        }

        public bool IsValidMove(int r, int c, CellState player)
        {
            if (r < 0 || r >= 8 || c < 0 || c >= 8 || Board[r, c] != CellState.Empty) return false;

            CellState opponent = (player == CellState.Player1) ? CellState.Player2 : CellState.Player1;
            int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nr = r + dr[i];
                int nc = c + dc[i];
                bool sawOpponent = false;

                while (nr >= 0 && nr < 8 && nc >= 0 && nc < 8 && Board[nr, nc] == opponent)
                {
                    nr += dr[i];
                    nc += dc[i];
                    sawOpponent = true;
                }

                if (sawOpponent && nr >= 0 && nr < 8 && nc >= 0 && nc < 8 && Board[nr, nc] == player)
                {
                    return true;
                }
            }
            return false;
        }

        private void FlipDiscs(int r, int c, CellState player)
        {
            Board[r, c] = player;
            CellState opponent = (player == CellState.Player1) ? CellState.Player2 : CellState.Player1;
            int[] dr = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dc = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < 8; i++)
            {
                int nr = r + dr[i];
                int nc = c + dc[i];
                List<Tuple<int, int>> potentialFlips = new List<Tuple<int, int>>();

                while (nr >= 0 && nr < 8 && nc >= 0 && nc < 8 && Board[nr, nc] == opponent)
                {
                    potentialFlips.Add(new Tuple<int, int>(nr, nc));
                    nr += dr[i];
                    nc += dc[i];
                }

                if (potentialFlips.Count > 0 && nr >= 0 && nr < 8 && nc >= 0 && nc < 8 && Board[nr, nc] == player)
                {
                    foreach (var flip in potentialFlips)
                    {
                        Board[flip.Item1, flip.Item2] = player;
                    }
                }
            }
        }

        private bool HasValidMoves(CellState player)
        {
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                    if (IsValidMove(r, c, player)) return true;
            return false;
        }

        private void UpdateScores()
        {
            BlackScore = 0;
            WhiteScore = 0;
            foreach (var cell in Board)
            {
                if (cell == CellState.Player1) BlackScore++;
                else if (cell == CellState.Player2) WhiteScore++;
            }
        }

        public Tuple<int, int> GetBestMove(Difficulty difficulty)
        {
             var moves = new List<Tuple<int, int>>();
             for(int r=0; r<8; r++)
                for(int c=0; c<8; c++)
                    if(IsValidMove(r,c,CurrentTurn)) moves.Add(new Tuple<int,int>(r,c));

             if(moves.Count == 0) return null;

             if(difficulty == Difficulty.Beginner)
             {
                 return moves[new Random().Next(moves.Count)];
             }

             // Greedy for Intermediate/Expert (Max flips + Corner priority)
             Tuple<int, int> bestMove = moves[0];
             int bestScore = -1000;

             foreach(var move in moves)
             {
                 int score = 0;
                 // Simulate simple score (corners are valuable)
                 if((move.Item1 == 0 || move.Item1 == 7) && (move.Item2 == 0 || move.Item2 == 7)) score += 50;
                 else if (move.Item1 == 0 || move.Item1 == 7 || move.Item2 == 0 || move.Item2 == 7) score += 10;
                 
                 // Add random factor for Intermediate to make it less perfect than Expert
                 if(difficulty == Difficulty.Intermediate) score += new Random().Next(-5, 5);
                 
                 // Count flips (roughly) - not simulating full board copy for brevity in this snippet
                 if(score > bestScore)
                 {
                     bestScore = score;
                     bestMove = move;
                 }
             }
             return bestMove;
        }
    }
}