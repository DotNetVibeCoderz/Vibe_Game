using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace RubikCube3D
{
    public enum Face
    {
        Front, Back, Up, Down, Left, Right
    }

    public class Cubie
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public SKColor[] FaceColors { get; set; }

        public Cubie(int x, int y, int z)
        {
            X = x; Y = y; Z = z;
            FaceColors = new SKColor[6];
            // Indices: 0:Front, 1:Back, 2:Up, 3:Down, 4:Left, 5:Right
            for (int i = 0; i < 6; i++) FaceColors[i] = SKColors.Black;
        }

        public Cubie Clone()
        {
            var c = new Cubie(X, Y, Z);
            Array.Copy(FaceColors, c.FaceColors, 6);
            return c;
        }
    }

    public class CubeModel
    {
        public int Size { get; private set; } = 3;
        public List<Cubie> Cubies { get; private set; }
        public Stack<string> MoveHistory { get; private set; } = new Stack<string>();
        public Stack<string> RedoStack { get; private set; } = new Stack<string>();

        public CubeModel(int size = 3)
        {
            Size = size;
            Reset();
        }

        public void Reset()
        {
            Cubies = new List<Cubie>();
            
            // Generate centered coordinates
            // Formula: 2 * i - (Size - 1)
            // Size 3: 0->-2, 1->0, 2->2. Range [-2, 2]
            // Size 2: 0->-1, 1->1. Range [-1, 1]
            // Size 4: 0->-3, 1->-1, 2->1, 3->3. Range [-3, 3]

            for (int x = 0; x < Size; x++)
            {
                for (int y = 0; y < Size; y++)
                {
                    for (int z = 0; z < Size; z++)
                    {
                        int cx = 2 * x - (Size - 1);
                        int cy = 2 * y - (Size - 1);
                        int cz = 2 * z - (Size - 1);
                        Cubies.Add(new Cubie(cx, cy, cz)); 
                    }
                }
            }
            MoveHistory.Clear();
            RedoStack.Clear();
            
            foreach(var c in Cubies) UpdateColors(c);
        }

        private void UpdateColors(Cubie c)
        {
            // Find bounds
            int minX = Cubies.Min(cub => cub.X);
            int maxX = Cubies.Max(cub => cub.X);
            int minY = Cubies.Min(cub => cub.Y);
            int maxY = Cubies.Max(cub => cub.Y);
            int minZ = Cubies.Min(cub => cub.Z);
            int maxZ = Cubies.Max(cub => cub.Z);

            // Front (Z max) - Green
            if (c.Z == maxZ) c.FaceColors[0] = SKColors.Green;
            else c.FaceColors[0] = SKColors.Black;

            // Back (Z min) - Blue
            if (c.Z == minZ) c.FaceColors[1] = SKColors.Blue;
            else c.FaceColors[1] = SKColors.Black;

            // Up (Y max) - White
            if (c.Y == maxY) c.FaceColors[2] = SKColors.White;
            else c.FaceColors[2] = SKColors.Black;

            // Down (Y min) - Yellow
            if (c.Y == minY) c.FaceColors[3] = SKColors.Yellow;
            else c.FaceColors[3] = SKColors.Black;

            // Left (X min) - Orange
            if (c.X == minX) c.FaceColors[4] = SKColors.Orange;
            else c.FaceColors[4] = SKColors.Black;

            // Right (X max) - Red
            if (c.X == maxX) c.FaceColors[5] = SKColors.Red;
            else c.FaceColors[5] = SKColors.Black;
        }

        public void PerformMove(string move, bool record = true)
        {
            if (string.IsNullOrEmpty(move)) return;

            bool counterClockwise = move.EndsWith("'");
            char face = move[0];
            
            RotateFace(face, counterClockwise);

            if (record)
            {
                MoveHistory.Push(move);
                RedoStack.Clear();
            }
        }

        private void RotateFace(char face, bool ccw)
        {
            List<Cubie> layer = new List<Cubie>();
            
            int minX = Cubies.Min(c => c.X); int maxX = Cubies.Max(c => c.X);
            int minY = Cubies.Min(c => c.Y); int maxY = Cubies.Max(c => c.Y);
            int minZ = Cubies.Min(c => c.Z); int maxZ = Cubies.Max(c => c.Z);

            switch (char.ToUpper(face))
            {
                case 'U': layer = Cubies.Where(c => c.Y == maxY).ToList(); break;
                case 'D': layer = Cubies.Where(c => c.Y == minY).ToList(); break;
                case 'L': layer = Cubies.Where(c => c.X == minX).ToList(); break;
                case 'R': layer = Cubies.Where(c => c.X == maxX).ToList(); break;
                case 'F': layer = Cubies.Where(c => c.Z == maxZ).ToList(); break;
                case 'B': layer = Cubies.Where(c => c.Z == minZ).ToList(); break;
            }

            foreach (var c in layer)
            {
                int x = c.X; int y = c.Y; int z = c.Z;
                
                // Geometry Rotation Formulas (Standard Right-Handed, Y-Up)
                // U (CW): Front->Left => (x,z)->(-z,x)
                // D (CW): Front->Right => (x,z)->(z,-x)
                // F (CW): Up->Right => (x,y)->(y,-x)
                // B (CW): Up->Left => (x,y)->(-y,x)
                // R (CW): Front->Up => (y,z)->(z,-y)
                // L (CW): Front->Down => (y,z)->(-z,y)

                if (char.ToUpper(face) == 'U') // Rotate around Y axis
                {
                    if (!ccw) { c.X = -z; c.Z = x; RotateColors(c, 'Y', true); }
                    else      { c.X = z; c.Z = -x; RotateColors(c, 'Y', false); }
                }
                else if (char.ToUpper(face) == 'D') // Rotate around Y axis
                {
                    if (!ccw) { c.X = z; c.Z = -x; RotateColors(c, 'Y', false); }
                    else      { c.X = -z; c.Z = x; RotateColors(c, 'Y', true); }
                }
                else if (char.ToUpper(face) == 'F') // Rotate around Z axis
                {
                    if (!ccw) { c.X = y; c.Y = -x; RotateColors(c, 'Z', false); }
                    else      { c.X = -y; c.Y = x; RotateColors(c, 'Z', true); }
                }
                 else if (char.ToUpper(face) == 'B') // Rotate around Z axis
                {
                    if (!ccw) { c.X = -y; c.Y = x; RotateColors(c, 'Z', true); }
                    else      { c.X = y; c.Y = -x; RotateColors(c, 'Z', false); }
                }
                else if (char.ToUpper(face) == 'R') // Rotate around X axis
                {
                    if (!ccw) { c.Y = z; c.Z = -y; RotateColors(c, 'X', false); }
                    else      { c.Y = -z; c.Z = y; RotateColors(c, 'X', true); }
                }
                 else if (char.ToUpper(face) == 'L') // Rotate around X axis
                {
                    if (!ccw) { c.Y = -z; c.Z = y; RotateColors(c, 'X', true); }
                    else      { c.Y = z; c.Z = -y; RotateColors(c, 'X', false); }
                }
            }
        }

        private void RotateColors(Cubie c, char axis, bool ccw)
        {
            var old = (SKColor[])c.FaceColors.Clone();
            // Indices: 0:Front, 1:Back, 2:Up, 3:Down, 4:Left, 5:Right
            
            if (axis == 'Y') // U/D
            {
                if (!ccw) // CW geometry (Front->Right)
                {
                    c.FaceColors[0] = old[4]; // F <- L
                    c.FaceColors[4] = old[1]; // L <- B
                    c.FaceColors[1] = old[5]; // B <- R
                    c.FaceColors[5] = old[0]; // R <- F
                }
                else // CCW geometry (Front->Left)
                {
                    c.FaceColors[0] = old[5]; // F <- R
                    c.FaceColors[5] = old[1]; // R <- B
                    c.FaceColors[1] = old[4]; // B <- L
                    c.FaceColors[4] = old[0]; // L <- F
                }
            }
            else if (axis == 'X') // L/R
            {
                if (!ccw) // CW geometry (Up->Front)
                {
                    c.FaceColors[0] = old[3]; // F <- D
                    c.FaceColors[3] = old[1]; // D <- B
                    c.FaceColors[1] = old[2]; // B <- U
                    c.FaceColors[2] = old[0]; // U <- F
                }
                else // CCW geometry (Up->Back)
                {
                    c.FaceColors[0] = old[2];
                    c.FaceColors[2] = old[1];
                    c.FaceColors[1] = old[3];
                    c.FaceColors[3] = old[0];
                }
            }
            else if (axis == 'Z') // F/B
            {
                 if (!ccw) // CW geometry (Up->Right)
                {
                    c.FaceColors[2] = old[4]; // U <- L
                    c.FaceColors[4] = old[3]; // L <- D
                    c.FaceColors[3] = old[5]; // D <- R
                    c.FaceColors[5] = old[2]; // R <- U
                }
                else // CCW geometry (Up->Left)
                {
                    c.FaceColors[2] = old[5];
                    c.FaceColors[5] = old[3];
                    c.FaceColors[3] = old[4];
                    c.FaceColors[4] = old[2];
                }
            }
        }

        public void Undo()
        {
            if (MoveHistory.Count == 0) return;
            string lastMove = MoveHistory.Pop();
            RedoStack.Push(lastMove);

            string inverted = lastMove.EndsWith("'") ? lastMove.TrimEnd('\'') : lastMove + "'";
            PerformMove(inverted, false);
        }

        public void Redo()
        {
            if (RedoStack.Count == 0) return;
            string move = RedoStack.Pop();
            PerformMove(move, true);
        }

        public void Scramble(int moves = 20)
        {
            string[] possibleMoves = { "F", "B", "U", "D", "L", "R", "F'", "B'", "U'", "D'", "L'", "R'" };
            Random r = new Random();
            for (int i = 0; i < moves; i++)
            {
                string m = possibleMoves[r.Next(possibleMoves.Length)];
                PerformMove(m);
            }
        }
    }
}