using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Maze
{
    public class Sprite
    {
        public double X;
        public double Y;
        public int TextureId; // 0 = Treasure
        public bool Collected = false;
        
        public Sprite(double x, double y, int id)
        {
            X = x;
            Y = y;
            TextureId = id;
        }
    }

    public class GameEngine
    {
        public int MapWidth = 24;
        public int MapHeight = 24;
        public int[,] WorldMap;
        
        public double PosX = 22.0, PosY = 12.0;
        public double DirX = -1.0, DirY = 0.0;
        public double PlaneX = 0.0, PlaneY = 0.66;
        public int Points = 0;
        public int Level = 1;
        public bool LevelFinished = false;

        public Point FinishPoint;
        public Point StartPoint;

        public DirectBitmap WallTexture;
        public DirectBitmap DoorTexture;
        public DirectBitmap TreasureTexture;
        
        public List<Sprite> Sprites = new List<Sprite>();
        private double[] _zBuffer; // Untuk sprite casting depth check

        public GameEngine()
        {
            // Initial Load will trigger Level 1
        }
        
        private void InitTextures(int level)
        {
            // Ubah warna tembok tiap level biar gak bosan
            Random r = new Random(level);
            Color wallColor = Color.FromArgb(r.Next(100, 200), r.Next(100, 200), r.Next(100, 200));
            Color mortarColor = Color.FromArgb(50, 50, 50);
            
            WallTexture = AssetGen.GenerateWallTexture(64, wallColor, mortarColor);
            DoorTexture = AssetGen.GeneratePixelArtTexture(64, "Door");
            TreasureTexture = AssetGen.GeneratePixelArtTexture(64, "Treasure");
        }

        public void LoadLevel(int level)
        {
            Level = level;
            LevelFinished = false;
            InitTextures(level);
            Points = (level == 1) ? 0 : Points; // Keep points
            
            // Generate Valid Maze
            bool validMap = false;
            while (!validMap)
            {
                GenerateMaze(level);
                validMap = IsPathValid();
            }
        }

        private void GenerateMaze(int level)
        {
            WorldMap = new int[MapWidth, MapHeight];
            Sprites.Clear();
            Random rnd = new Random();

            // Fill borders
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    if (x == 0 || x == MapWidth - 1 || y == 0 || y == MapHeight - 1)
                        WorldMap[x, y] = 1;
                    else
                        WorldMap[x, y] = (rnd.Next(100) < 20) ? 1 : 0; // 20% walls
                }
            }

            // Set Start
            StartPoint = new Point(1, 1);
            WorldMap[1, 1] = 0;
            WorldMap[1, 2] = 0; // Ensure breathing room
            WorldMap[2, 1] = 0;
            PosX = 1.5; PosY = 1.5;
            DirX = -1.0; DirY = 0.0;
            PlaneX = 0.0; PlaneY = 0.66;

            // Set Finish (Exit Door)
            // Let's ensure finish is far enough
            FinishPoint = new Point(MapWidth - 2, MapHeight - 2);
            WorldMap[MapWidth - 2, MapHeight - 2] = 2; // 2 = Door ID
            WorldMap[MapWidth - 3, MapHeight - 2] = 0; // Ensure access

            // Spawn Treasures (Sprites)
            int treasureCount = 5 + level;
            for(int i=0; i<treasureCount; i++)
            {
                int tx = rnd.Next(1, MapWidth - 1);
                int ty = rnd.Next(1, MapHeight - 1);
                if(WorldMap[tx, ty] == 0 && (tx != 1 || ty != 1))
                {
                    Sprites.Add(new Sprite(tx + 0.5, ty + 0.5, 0)); // Center of tile
                }
            }
        }

        private bool IsPathValid()
        {
            // BFS check from Start to Finish
            bool[,] visited = new bool[MapWidth, MapHeight];
            Queue<Point> queue = new Queue<Point>();
            
            queue.Enqueue(StartPoint);
            visited[StartPoint.X, StartPoint.Y] = true;

            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0 };

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                if (p.X == FinishPoint.X && p.Y == FinishPoint.Y) return true;

                for (int i = 0; i < 4; i++)
                {
                    int nx = p.X + dx[i];
                    int ny = p.Y + dy[i];

                    if (nx >= 0 && nx < MapWidth && ny >= 0 && ny < MapHeight)
                    {
                        // Walkable: 0 (Floor) or 2 (Door/Finish)
                        // Note: 1 is wall.
                        if (!visited[nx, ny] && (WorldMap[nx, ny] == 0 || WorldMap[nx, ny] == 2))
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Point(nx, ny));
                        }
                    }
                }
            }
            return false;
        }

        public void Update(bool up, bool down, bool left, bool right, double frameTime)
        {
            double moveSpeed = frameTime * 5.0; 
            double rotSpeed = frameTime * 3.0; 

            // Basic Collision Logic
            // Check radius 0.2
            if (up) Move(DirX * moveSpeed, DirY * moveSpeed);
            if (down) Move(-DirX * moveSpeed, -DirY * moveSpeed);
            
            if (right) Rotate(-rotSpeed);
            if (left) Rotate(rotSpeed);

            // Collect Treasures
            for (int i = Sprites.Count - 1; i >= 0; i--)
            {
                Sprite s = Sprites[i];
                double dist = Math.Sqrt(Math.Pow(PosX - s.X, 2) + Math.Pow(PosY - s.Y, 2));
                if (dist < 0.5) // Pickup range
                {
                    Points += 10;
                    Sprites.RemoveAt(i);
                    // Notify Main Form? We'll check diff in Form
                }
            }

            // Check Finish
            if ((int)PosX == FinishPoint.X && (int)PosY == FinishPoint.Y)
            {
                LevelFinished = true;
            }
        }

        private void Move(double dx, double dy)
        {
            // Simple wall collision check
            if (WorldMap[(int)(PosX + dx * 2), (int)PosY] != 1) PosX += dx; // *2 for buffer
            if (WorldMap[(int)PosX, (int)(PosY + dy * 2)] != 1) PosY += dy;
        }

        private void Rotate(double rot)
        {
            double oldDirX = DirX;
            DirX = DirX * Math.Cos(rot) - DirY * Math.Sin(rot);
            DirY = oldDirX * Math.Sin(rot) + DirY * Math.Cos(rot);
            double oldPlaneX = PlaneX;
            PlaneX = PlaneX * Math.Cos(rot) - PlaneY * Math.Sin(rot);
            PlaneY = oldPlaneX * Math.Sin(rot) + PlaneY * Math.Cos(rot);
        }

        public void Render(DirectBitmap buffer, int w, int h)
        {
            if (_zBuffer == null || _zBuffer.Length != w) _zBuffer = new double[w];

            // 1. FLOOR & CEILING
            for(int y=0; y<h; y++)
            {
                int c = (y < h/2) ? Color.FromArgb(20,20,30).ToArgb() : Color.FromArgb(50,50,50).ToArgb();
                for(int x=0; x<w; x++) buffer.SetPixel(x, y, c);
            }

            // 2. WALL CASTING
            for (int x = 0; x < w; x++)
            {
                double cameraX = 2 * x / (double)w - 1;
                double rayDirX = DirX + PlaneX * cameraX;
                double rayDirY = DirY + PlaneY * cameraX;

                int mapX = (int)PosX;
                int mapY = (int)PosY;

                double sideDistX, sideDistY;
                double deltaDistX = (rayDirX == 0) ? 1e30 : Math.Abs(1 / rayDirX);
                double deltaDistY = (rayDirY == 0) ? 1e30 : Math.Abs(1 / rayDirY);
                double perpWallDist;

                int stepX, stepY;
                int hit = 0;
                int side = 0;

                if (rayDirX < 0) { stepX = -1; sideDistX = (PosX - mapX) * deltaDistX; }
                else { stepX = 1; sideDistX = (mapX + 1.0 - PosX) * deltaDistX; }
                if (rayDirY < 0) { stepY = -1; sideDistY = (PosY - mapY) * deltaDistY; }
                else { stepY = 1; sideDistY = (mapY + 1.0 - PosY) * deltaDistY; }

                int wallID = 0;
                while (hit == 0)
                {
                    if (sideDistX < sideDistY) { sideDistX += deltaDistX; mapX += stepX; side = 0; }
                    else { sideDistY += deltaDistY; mapY += stepY; side = 1; }
                    
                    if (WorldMap[mapX, mapY] > 0)
                    {
                        hit = 1;
                        wallID = WorldMap[mapX, mapY];
                    }
                }

                if (side == 0) perpWallDist = (sideDistX - deltaDistX);
                else perpWallDist = (sideDistY - deltaDistY);
                
                _zBuffer[x] = perpWallDist; // Store Z for sprite casting

                int lineHeight = (int)(h / perpWallDist);
                int drawStart = -lineHeight / 2 + h / 2;
                if (drawStart < 0) drawStart = 0;
                int drawEnd = lineHeight / 2 + h / 2;
                if (drawEnd >= h) drawEnd = h - 1;

                // Texturing
                double wallX;
                if (side == 0) wallX = PosY + perpWallDist * rayDirY;
                else wallX = PosX + perpWallDist * rayDirX;
                wallX -= Math.Floor(wallX);

                int texX = (int)(wallX * 64);
                if (side == 0 && rayDirX > 0) texX = 64 - texX - 1;
                if (side == 1 && rayDirY < 0) texX = 64 - texX - 1;

                DirectBitmap texToUse = (wallID == 2) ? DoorTexture : WallTexture;
                
                double step = 1.0 * 64 / lineHeight;
                double texPos = (drawStart - h / 2 + lineHeight / 2) * step;

                for (int y = drawStart; y < drawEnd; y++)
                {
                    int texY = (int)texPos & 63;
                    texPos += step;
                    int color = texToUse.GetPixel(texX, texY);
                    if (side == 1 && wallID == 1) color = (color >> 1) & 8355711; // Shading
                    buffer.SetPixel(x, y, color);
                }
            }

            // 3. SPRITE CASTING
            // Sort sprites by distance
            Sprites.Sort((a, b) => 
            {
                double distA = (PosX - a.X) * (PosX - a.X) + (PosY - a.Y) * (PosY - a.Y);
                double distB = (PosX - b.X) * (PosX - b.X) + (PosY - b.Y) * (PosY - b.Y);
                return distB.CompareTo(distA); // Far to near
            });

            foreach(var sprite in Sprites)
            {
                double spriteX = sprite.X - PosX;
                double spriteY = sprite.Y - PosY;

                double invDet = 1.0 / (PlaneX * DirY - DirX * PlaneY);
                double transformX = invDet * (DirY * spriteX - DirX * spriteY);
                double transformY = invDet * (-PlaneY * spriteX + PlaneX * spriteY); // Depth

                int spriteScreenX = (int)((w / 2) * (1 + transformX / transformY));

                int spriteHeight = Math.Abs((int)(h / (transformY))); 
                int drawStartY = -spriteHeight / 2 + h / 2;
                if (drawStartY < 0) drawStartY = 0;
                int drawEndY = spriteHeight / 2 + h / 2;
                if (drawEndY >= h) drawEndY = h - 1;

                int spriteWidth = Math.Abs((int)(h / (transformY)));
                int drawStartX = -spriteWidth / 2 + spriteScreenX;
                if (drawStartX < 0) drawStartX = 0;
                int drawEndX = spriteWidth / 2 + spriteScreenX;
                if (drawEndX >= w) drawEndX = w - 1;

                DirectBitmap spriteTex = TreasureTexture; // Assuming all sprites are treasure for now

                for (int stripe = drawStartX; stripe < drawEndX; stripe++)
                {
                    int texX = (int)(256 * (stripe - (-spriteWidth / 2 + spriteScreenX)) * 64 / spriteWidth) / 256;
                    
                    if (transformY > 0 && stripe > 0 && stripe < w && transformY < _zBuffer[stripe])
                    {
                        for (int y = drawStartY; y < drawEndY; y++)
                        {
                            int d = (y) * 256 - h * 128 + spriteHeight * 128;
                            int texY = ((d * 64) / spriteHeight) / 256;
                            int color = spriteTex.GetPixel(texX, texY);
                            if ((color & 0x00FFFFFF) != 0) // Basic transparency check (black)
                                buffer.SetPixel(stripe, y, color);
                        }
                    }
                }
            }
        }
    }
}