using System;
using System.Collections.Generic;
using System.Linq;

namespace BoardGames.Models
{
    public class CheckersLogic
    {
        public enum PieceType { Empty, Man, King }
        public struct Piece
        {
            public CellState Player;
            public PieceType Type;
        }

        public Piece[,] Board { get; private set; }
        public CellState CurrentTurn { get; private set; }
        public bool IsGameOver { get; private set; }
        public CellState Winner { get; private set; }

        public CheckersLogic()
        {
            NewGame();
        }

        public void NewGame()
        {
            Board = new Piece[8, 8];
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    Board[r, c] = new Piece { Player = CellState.Empty, Type = PieceType.Empty };
                    if ((r + c) % 2 == 1)
                    {
                        if (r < 3) Board[r, c] = new Piece { Player = CellState.Player2, Type = PieceType.Man }; // White/O
                        else if (r > 4) Board[r, c] = new Piece { Player = CellState.Player1, Type = PieceType.Man }; // Black/X
                    }
                }
            CurrentTurn = CellState.Player1;
            IsGameOver = false;
        }

        // Simplified Move Logic (Source R,C -> Dest R,C)
        public bool MakeMove(int r1, int c1, int r2, int c2)
        {
            if (IsGameOver) return false;
            if (Board[r1, c1].Player != CurrentTurn) return false;

            var validMoves = GetValidMoves(CurrentTurn);
            // Check if move is in valid moves
            var move = validMoves.FirstOrDefault(m => m.FromR == r1 && m.FromC == c1 && m.ToR == r2 && m.ToC == c2);
            
            if (move == null) return false;

            ExecuteMove(move);
            
            // Check for double jump if captured
            if (move.IsCapture)
            {
                var followUpMoves = GetJumps(move.ToR, move.ToC, CurrentTurn, Board[move.ToR, move.ToC].Type);
                if (followUpMoves.Count > 0)
                {
                    // Force next turn to be same player, restricted to this piece
                    // Simplified: We just swap turns normally here to save complexity for this demo
                }
            }

            CurrentTurn = (CurrentTurn == CellState.Player1) ? CellState.Player2 : CellState.Player1;
            CheckWinCondition();
            return true;
        }

        private void ExecuteMove(Move move)
        {
            Board[move.ToR, move.ToC] = Board[move.FromR, move.FromC];
            Board[move.FromR, move.FromC] = new Piece { Player = CellState.Empty, Type = PieceType.Empty };

            if (move.IsCapture)
            {
                int midR = (move.FromR + move.ToR) / 2;
                int midC = (move.FromC + move.ToC) / 2;
                Board[midR, midC] = new Piece { Player = CellState.Empty, Type = PieceType.Empty };
            }

            // King promotion
            if (CurrentTurn == CellState.Player1 && move.ToR == 0) Board[move.ToR, move.ToC].Type = PieceType.King;
            if (CurrentTurn == CellState.Player2 && move.ToR == 7) Board[move.ToR, move.ToC].Type = PieceType.King;
        }

        private void CheckWinCondition()
        {
            bool p1HasMoves = GetValidMoves(CellState.Player1).Count > 0;
            bool p2HasMoves = GetValidMoves(CellState.Player2).Count > 0;

            if (!p1HasMoves) { Winner = CellState.Player2; IsGameOver = true; }
            else if (!p2HasMoves) { Winner = CellState.Player1; IsGameOver = true; }
        }

        public class Move
        {
            public int FromR, FromC, ToR, ToC;
            public bool IsCapture;
        }

        public List<Move> GetValidMoves(CellState player)
        {
            List<Move> jumps = new List<Move>();
            List<Move> slides = new List<Move>();

            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    if (Board[r, c].Player == player)
                    {
                        jumps.AddRange(GetJumps(r, c, player, Board[r, c].Type));
                        slides.AddRange(GetSlides(r, c, player, Board[r, c].Type));
                    }
                }
            }

            // Forced capture rule
            return jumps.Count > 0 ? jumps : slides;
        }

        private List<Move> GetSlides(int r, int c, CellState player, PieceType type)
        {
            List<Move> moves = new List<Move>();
            int[] dr = (type == PieceType.King) ? new[] { -1, 1 } : (player == CellState.Player1 ? new[] { -1 } : new[] { 1 });
            int[] dc = { -1, 1 };

            foreach (var dR in dr)
            {
                foreach (var dC in dc)
                {
                    int nr = r + dR, nc = c + dC;
                    if (nr >= 0 && nr < 8 && nc >= 0 && nc < 8 && Board[nr, nc].Player == CellState.Empty)
                    {
                        moves.Add(new Move { FromR = r, FromC = c, ToR = nr, ToC = nc, IsCapture = false });
                    }
                }
            }
            return moves;
        }

        private List<Move> GetJumps(int r, int c, CellState player, PieceType type)
        {
            List<Move> moves = new List<Move>();
            CellState opponent = (player == CellState.Player1) ? CellState.Player2 : CellState.Player1;
            int[] dr = (type == PieceType.King) ? new[] { -1, 1 } : (player == CellState.Player1 ? new[] { -1 } : new[] { 1 });
            int[] dc = { -1, 1 };

            foreach (var dR in dr)
            {
                foreach (var dC in dc)
                {
                    int midR = r + dR, midC = c + dC;
                    int toR = r + dR * 2, toC = c + dC * 2;

                    if (toR >= 0 && toR < 8 && toC >= 0 && toC < 8 &&
                        Board[midR, midC].Player == opponent &&
                        Board[toR, toC].Player == CellState.Empty)
                    {
                        moves.Add(new Move { FromR = r, FromC = c, ToR = toR, ToC = toC, IsCapture = true });
                    }
                }
            }
            return moves;
        }

        public Move GetBestMove(Difficulty difficulty)
        {
            var moves = GetValidMoves(CurrentTurn);
            if (moves.Count == 0) return null;
            return moves[new Random().Next(moves.Count)];
        }
    }
}