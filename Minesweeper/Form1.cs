
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Minesweeper
{
    public partial class Form1 : Form
    {
        // Game Constants
        private const int CELL_SIZE = 25;
        
        // Difficulty Settings
        private int ROWS = 9;
        private int COLS = 9;
        private int MINES = 10;

        // Custom Difficulty Enum
        private enum Difficulty
        {
            Beginner,
            Intermediate,
            Expert
        }

        private Difficulty currentDifficulty = Difficulty.Beginner;

        // Game State
        private struct Cell
        {
            public bool IsMine;
            public bool IsRevealed;
            public bool IsFlagged;
            public int NeighborMines;
        }

        private Cell[,] grid;
        private Button[,] buttons;
        private bool isFirstClick;
        private bool isGameOver;
        private int flagsPlaced;
        private int timeElapsed;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            StartNewGame(Difficulty.Beginner);
        }

        private void StartNewGame(Difficulty difficulty)
        {
            currentDifficulty = difficulty;
            
            switch (difficulty)
            {
                case Difficulty.Beginner:
                    ROWS = 9; COLS = 9; MINES = 10;
                    break;
                case Difficulty.Intermediate:
                    ROWS = 16; COLS = 16; MINES = 40;
                    break;
                case Difficulty.Expert:
                    ROWS = 16; COLS = 30; MINES = 99;
                    break;
            }

            // Adjust Form Size
            int boardWidth = COLS * CELL_SIZE;
            int boardHeight = ROWS * CELL_SIZE;
            
            // Allow space for borders and top panel
            this.ClientSize = new Size(boardWidth + 20, boardHeight + 80 + 20);
            
            // Re-center face button
            btnFace.Location = new Point((panelTop.Width - btnFace.Width) / 2, btnFace.Location.Y);

            InitializeBoard();
        }

        private void InitializeBoard()
        {
            // Reset Game State
            isFirstClick = true;
            isGameOver = false;
            flagsPlaced = 0;
            timeElapsed = 0;
            
            lblMines.Text = MINES.ToString("D3");
            lblTime.Text = "000";
            btnFace.Text = "🙂"; // Smiley
            gameTimer.Stop();

            // Clear old buttons
            panelBoard.Controls.Clear();

            // Initialize Arrays
            grid = new Cell[ROWS, COLS];
            buttons = new Button[ROWS, COLS];

            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    Button btn = new Button();
                    btn.Size = new Size(CELL_SIZE, CELL_SIZE);
                    btn.Location = new Point(c * CELL_SIZE, r * CELL_SIZE);
                    btn.FlatStyle = FlatStyle.System; // Classic look
                    btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
                    btn.Tag = new Point(r, c);
                    btn.MouseDown += Cell_MouseDown;
                    btn.MouseUp += Cell_MouseUp; // Handle smiley reset
                    
                    panelBoard.Controls.Add(btn);
                    buttons[r, c] = btn;
                    
                    // Initialize logical cell
                    grid[r, c] = new Cell();
                }
            }
        }

        private void PlaceMines(int excludeRow, int excludeCol)
        {
            Random rnd = new Random();
            int minesPlaced = 0;

            while (minesPlaced < MINES)
            {
                int r = rnd.Next(ROWS);
                int c = rnd.Next(COLS);

                // Don't place mine on existing mine or first click area
                if (!grid[r, c].IsMine && !(r == excludeRow && c == excludeCol))
                {
                    grid[r, c].IsMine = true;
                    minesPlaced++;
                }
            }

            CalculateNeighbors();
            
            // Start Timer
            gameTimer.Start();
        }

        private void CalculateNeighbors()
        {
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    if (grid[r, c].IsMine) continue;

                    int count = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            int nr = r + i;
                            int nc = c + j;
                            if (nr >= 0 && nr < ROWS && nc >= 0 && nc < COLS && grid[nr, nc].IsMine)
                            {
                                count++;
                            }
                        }
                    }
                    grid[r, c].NeighborMines = count;
                }
            }
        }

        private void Cell_MouseDown(object sender, MouseEventArgs e)
        {
            if (isGameOver) return;
            
            Button btn = sender as Button;
            Point p = (Point)btn.Tag;
            int r = p.X;
            int c = p.Y;

            btnFace.Text = "😮"; // O face on click

            if (e.Button == MouseButtons.Left)
            {
                if (grid[r, c].IsFlagged) return;

                if (isFirstClick)
                {
                    PlaceMines(r, c);
                    isFirstClick = false;
                }

                RevealCell(r, c);
                CheckWin();
            }
            else if (e.Button == MouseButtons.Right)
            {
                ToggleFlag(r, c);
            }
        }

        private void Cell_MouseUp(object sender, MouseEventArgs e)
        {
             if (!isGameOver) btnFace.Text = "🙂";
        }

        private void ToggleFlag(int r, int c)
        {
            if (grid[r, c].IsRevealed) return;

            if (grid[r, c].IsFlagged)
            {
                grid[r, c].IsFlagged = false;
                buttons[r, c].Text = "";
                buttons[r, c].ForeColor = Color.Black;
                flagsPlaced--;
            }
            else
            {
                grid[r, c].IsFlagged = true;
                buttons[r, c].Text = "🚩";
                buttons[r, c].ForeColor = Color.Red;
                flagsPlaced++;
            }

            lblMines.Text = (MINES - flagsPlaced).ToString("D3");
        }

        private void RevealCell(int r, int c)
        {
            if (r < 0 || r >= ROWS || c < 0 || c >= COLS || grid[r, c].IsRevealed || grid[r, c].IsFlagged)
                return;

            grid[r, c].IsRevealed = true;
            Button btn = buttons[r, c];
            
            // Styling for revealed cell
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.Gray;
            btn.BackColor = Color.LightGray;

            if (grid[r, c].IsMine)
            {
                TriggerGameOver(false);
                btn.BackColor = Color.Red;
                btn.Text = "💣";
            }
            else
            {
                int input = grid[r, c].NeighborMines;
                if (input > 0)
                {
                    btn.Text = input.ToString();
                    btn.ForeColor = GetColorForNumber(input);
                    btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold); 
                }
                else
                {
                    // Recursive flood fill for empty cells
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            RevealCell(r + i, c + j);
                        }
                    }
                }
            }
        }

        private Color GetColorForNumber(int number)
        {
            switch (number)
            {
                case 1: return Color.Blue;
                case 2: return Color.Green;
                case 3: return Color.Red;
                case 4: return Color.DarkBlue;
                case 5: return Color.DarkRed;
                case 6: return Color.Teal;
                case 7: return Color.Black;
                case 8: return Color.Gray;
                default: return Color.Black;
            }
        }

        private void CheckWin()
        {
            if (isGameOver) return;

            bool allRevealed = true;
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    if (!grid[r, c].IsMine && !grid[r, c].IsRevealed)
                    {
                        allRevealed = false;
                        break;
                    }
                }
            }

            if (allRevealed)
            {
                TriggerGameOver(true);
            }
        }

        private void TriggerGameOver(bool won)
        {
            isGameOver = true;
            gameTimer.Stop();

            if (won)
            {
                btnFace.Text = "😎"; // Cool face
                lblMines.Text = "000";
                // Flag all mines visually
                for (int r = 0; r < ROWS; r++)
                {
                    for (int c = 0; c < COLS; c++)
                    {
                        if (grid[r, c].IsMine)
                        {
                            buttons[r, c].Text = "🚩";
                            buttons[r, c].ForeColor = Color.Red;
                        }
                    }
                }
                MessageBox.Show("Selamat! Kamu Menang! Jangan lupa pulsanya bos :D", "You Won!", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                btnFace.Text = "😵"; // Dead face
                // Reveal all mines
                for (int r = 0; r < ROWS; r++)
                {
                    for (int c = 0; c < COLS; c++)
                    {
                        if (grid[r, c].IsMine && !grid[r, c].IsRevealed)
                        {
                            buttons[r, c].Text = "💣";
                            buttons[r, c].ForeColor = Color.Black;
                        }
                        // Check for wrong flags
                        if (!grid[r, c].IsMine && grid[r, c].IsFlagged)
                        {
                             buttons[r, c].Text = "❌"; // Wrong flag
                             buttons[r, c].ForeColor = Color.Red;
                        }
                    }
                }
                MessageBox.Show("Boom! Sayang sekali. Coba lagi bang.", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            timeElapsed++;
            if (timeElapsed > 999) timeElapsed = 999;
            lblTime.Text = timeElapsed.ToString("D3");
        }

        // Menu Event Handlers
        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNewGame(currentDifficulty);
        }

        private void beginnerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNewGame(Difficulty.Beginner);
        }

        private void intermediateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNewGame(Difficulty.Intermediate);
        }

        private void expertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartNewGame(Difficulty.Expert);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Minesweeper Retro Edition\n\nCreated by Jacky the code bender (Gravicode Studios)\n\nEnjoy the game!", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnFace_Click(object sender, EventArgs e)
        {
            StartNewGame(currentDifficulty);
        }
    }
}
