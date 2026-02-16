using System;
using System.Collections.Generic;
using System.Linq;
using CrossWordsNet.Models;

namespace CrossWordsNet.Services
{
    public class CrosswordGenerator
    {
        private const int GridSize = 15;
        private char[,] _grid;
        private Random _random = new Random();

        public (List<CrosswordCell> cells, List<CrosswordWord> words) GenerateLevel()
        {
            _grid = new char[GridSize, GridSize];
            for (int r = 0; r < GridSize; r++)
                for (int c = 0; c < GridSize; c++)
                    _grid[r, c] = ' ';

            var placedWords = new List<CrosswordWord>();
            var availableWords = WordBank.Words.OrderBy(x => _random.Next()).ToList();
            
            // 1. Place the first word horizontally in the middle
            var first = availableWords[0];
            availableWords.RemoveAt(0);
            
            int startR = GridSize / 2;
            int startC = (GridSize - first.Word.Length) / 2;
            
            if (TryPlaceWord(first.Word, startR, startC, Direction.Horizontal))
            {
                placedWords.Add(new CrosswordWord { Word = first.Word, Clue = first.Clue, StartRow = startR, StartCol = startC, Direction = Direction.Horizontal });
            }

            // 2. Try to place more words by intersecting
            int maxWords = 12;
            int attempts = 0;

            while (placedWords.Count < maxWords && attempts < 1000 && availableWords.Count > 0)
            {
                attempts++;
                var candidate = availableWords[_random.Next(availableWords.Count)];
                
                // Find potential intersections
                bool placed = false;
                foreach (var placedWord in placedWords)
                {
                    // Find common letters
                    for (int i = 0; i < placedWord.Word.Length; i++)
                    {
                        char commonChar = placedWord.Word[i];
                        int pRow = placedWord.StartRow + (placedWord.Direction == Direction.Vertical ? i : 0);
                        int pCol = placedWord.StartCol + (placedWord.Direction == Direction.Horizontal ? i : 0);

                        for (int j = 0; j < candidate.Word.Length; j++)
                        {
                            if (candidate.Word[j] == commonChar)
                            {
                                // Potential intersection
                                Direction newDir = placedWord.Direction == Direction.Horizontal ? Direction.Vertical : Direction.Horizontal;
                                int newRow = pRow - (newDir == Direction.Vertical ? j : 0);
                                int newCol = pCol - (newDir == Direction.Horizontal ? j : 0);

                                if (TryPlaceWord(candidate.Word, newRow, newCol, newDir))
                                {
                                    placedWords.Add(new CrosswordWord 
                                    { 
                                        Word = candidate.Word, 
                                        Clue = candidate.Clue, 
                                        StartRow = newRow, 
                                        StartCol = newCol, 
                                        Direction = newDir 
                                    });
                                    availableWords.Remove(candidate);
                                    placed = true;
                                    break;
                                }
                            }
                        }
                        if (placed) break;
                    }
                    if (placed) break;
                }
            }
            
            // 3. Numbering Logic
            // Sort words by position (top-left to bottom-right)
            placedWords = placedWords.OrderBy(w => w.StartRow).ThenBy(w => w.StartCol).ToList();
            
            // Identify start cells
            var startCells = new Dictionary<(int r, int c), int>();
            int currentNumber = 1;

            // We iterate through grid to assign numbers sequentially like a real crossword
            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    bool startsAny = placedWords.Any(w => w.StartRow == r && w.StartCol == c);
                    if (startsAny)
                    {
                        startCells[(r, c)] = currentNumber;
                        currentNumber++;
                    }
                }
            }

            foreach(var w in placedWords)
            {
                if (startCells.ContainsKey((w.StartRow, w.StartCol)))
                {
                    w.Number = startCells[(w.StartRow, w.StartCol)];
                }
            }

            // 4. Create Cell List for UI
            var cells = new List<CrosswordCell>();
            for (int r = 0; r < GridSize; r++)
            {
                for (int c = 0; c < GridSize; c++)
                {
                    var cell = new CrosswordCell { Row = r, Col = c };
                    if (_grid[r, c] != ' ')
                    {
                        cell.IsBlock = false;
                        cell.Letter = _grid[r, c];
                        if (startCells.ContainsKey((r, c)))
                        {
                            cell.Number = startCells[(r, c)];
                        }
                    }
                    else
                    {
                        cell.IsBlock = true;
                    }
                    cells.Add(cell);
                }
            }

            return (cells, placedWords);
        }

        private bool TryPlaceWord(string word, int row, int col, Direction dir)
        {
            if (row < 0 || col < 0) return false;
            if (dir == Direction.Horizontal && col + word.Length > GridSize) return false;
            if (dir == Direction.Vertical && row + word.Length > GridSize) return false;

            // Check if valid placement
            for (int i = 0; i < word.Length; i++)
            {
                int r = row + (dir == Direction.Vertical ? i : 0);
                int c = col + (dir == Direction.Horizontal ? i : 0);

                char existing = _grid[r, c];
                
                // If cell is occupied, must match
                if (existing != ' ' && existing != word[i]) return false;

                // Check neighbors to avoid unintended adjacent words
                // Simplified: check immediate perpendicular neighbors if cell was empty
                if (existing == ' ')
                {
                    if (dir == Direction.Horizontal)
                    {
                        // Check Top and Bottom
                        if (r > 0 && _grid[r - 1, c] != ' ' && !IsPartOfVerticalWord(r-1, c)) return false; // Too simple check
                        if (r < GridSize - 1 && _grid[r + 1, c] != ' ' && !IsPartOfVerticalWord(r+1, c)) return false; 
                    }
                    else
                    {
                        // Check Left and Right
                        if (c > 0 && _grid[r, c - 1] != ' ' && !IsPartOfHorizontalWord(r, c-1)) return false;
                        if (c < GridSize - 1 && _grid[r, c + 1] != ' ' && !IsPartOfHorizontalWord(r, c+1)) return false;
                    }
                }
            }

            // Also check Before Start and After End
            if (dir == Direction.Horizontal)
            {
                if (col > 0 && _grid[row, col - 1] != ' ') return false;
                if (col + word.Length < GridSize && _grid[row, col + word.Length] != ' ') return false;
            }
            else
            {
                if (row > 0 && _grid[row - 1, col] != ' ') return false;
                if (row + word.Length < GridSize && _grid[row + word.Length, col] != ' ') return false;
            }

            // Commit to grid
            for (int i = 0; i < word.Length; i++)
            {
                int r = row + (dir == Direction.Vertical ? i : 0);
                int c = col + (dir == Direction.Horizontal ? i : 0);
                _grid[r, c] = word[i];
            }

            return true;
        }

        // Helper placeholders - robust check is hard without tracking word ownership per cell
        // For this simple generator, we just rely on the 'space' check.
        private bool IsPartOfVerticalWord(int r, int c) => false; 
        private bool IsPartOfHorizontalWord(int r, int c) => false;
    }
}