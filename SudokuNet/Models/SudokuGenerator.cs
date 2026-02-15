using System;

namespace SudokuNet.Models
{
    public class SudokuGenerator
    {
        private static Random _rand = new Random();

        public static int[,] GenerateSolution()
        {
            int[,] grid = new int[9, 9];
            FillDiagonal(grid);
            FillRemaining(grid, 0, 3);
            return grid;
        }

        private static void FillDiagonal(int[,] grid)
        {
            for (int i = 0; i < 9; i += 3)
                FillBox(grid, i, i);
        }

        private static void FillBox(int[,] grid, int row, int col)
        {
            int num;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    do
                    {
                        num = _rand.Next(1, 10);
                    }
                    while (!UnusedInBox(grid, row, col, num));
                    grid[row + i, col + j] = num;
                }
            }
        }

        private static bool UnusedInBox(int[,] grid, int rowStart, int colStart, int num)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (grid[rowStart + i, colStart + j] == num)
                        return false;
            return true;
        }

        private static bool CheckIfSafe(int[,] grid, int i, int j, int num)
        {
            return (UnusedInRow(grid, i, num) &&
                    UnusedInCol(grid, j, num) &&
                    UnusedInBox(grid, i - i % 3, j - j % 3, num));
        }

        private static bool UnusedInRow(int[,] grid, int i, int num)
        {
            for (int j = 0; j < 9; j++)
                if (grid[i, j] == num)
                    return false;
            return true;
        }

        private static bool UnusedInCol(int[,] grid, int j, int num)
        {
            for (int i = 0; i < 9; i++)
                if (grid[i, j] == num)
                    return false;
            return true;
        }

        private static bool FillRemaining(int[,] grid, int i, int j)
        {
            if (j >= 9 && i < 8)
            {
                i = i + 1;
                j = 0;
            }
            if (i >= 9 && j >= 9)
                return true;

            if (i < 3)
            {
                if (j < 3)
                    j = 3;
            }
            else if (i < 6)
            {
                if (j == (int)(i / 3) * 3)
                    j = j + 3;
            }
            else
            {
                if (j == 6)
                {
                    i = i + 1;
                    j = 0;
                    if (i >= 9)
                        return true;
                }
            }

            for (int num = 1; num <= 9; num++)
            {
                if (CheckIfSafe(grid, i, j, num))
                {
                    grid[i, j] = num;
                    if (FillRemaining(grid, i, j + 1))
                        return true;
                    grid[i, j] = 0;
                }
            }
            return false;
        }

        public static int[,] RemoveDigits(int[,] solution, int count)
        {
            int[,] puzzle = (int[,])solution.Clone();
            int remaining = count;
            while (remaining > 0)
            {
                int cellId = _rand.Next(0, 81);
                int i = cellId / 9;
                int j = cellId % 9;
                if (puzzle[i, j] != 0)
                {
                    puzzle[i, j] = 0;
                    remaining--;
                }
            }
            return puzzle;
        }
    }
}