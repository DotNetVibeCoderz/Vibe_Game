using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

namespace SpaceShooter
{
    public static class SpriteGenerator
    {
        public static void GenerateAssets()
        {
            if (!Directory.Exists("Assets")) Directory.CreateDirectory("Assets");

            CreatePlayerSprite("Assets/player.png");
            CreateScoutSprite("Assets/scout.png");
            CreateFighterSprite("Assets/fighter.png");
            CreateBossSprite("Assets/boss.png");
        }

        private static void CreatePlayerSprite(string path)
        {
            using (Bitmap bmp = new Bitmap(64, 64))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                PointF[] body = { new PointF(32, 5), new PointF(10, 55), new PointF(32, 45), new PointF(54, 55) };
                
                using (LinearGradientBrush b = new LinearGradientBrush(new Rectangle(0,0,64,64), Color.Cyan, Color.DarkBlue, 45f))
                    g.FillPolygon(b, body);
                
                g.DrawPolygon(new Pen(Color.White, 2), body);
                g.FillEllipse(Brushes.LightSkyBlue, 24, 25, 16, 12); // Cockpit
                
                // Wings
                g.FillRectangle(Brushes.SteelBlue, 5, 40, 15, 10);
                g.FillRectangle(Brushes.SteelBlue, 44, 40, 15, 10);
                
                bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static void CreateScoutSprite(string path)
        {
            using (Bitmap bmp = new Bitmap(48, 48))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillEllipse(Brushes.LimeGreen, 10, 5, 28, 38);
                g.DrawEllipse(Pens.White, 10, 5, 28, 38);
                g.FillRectangle(Brushes.Red, 18, 15, 12, 5); // Eye/Window
                bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static void CreateFighterSprite(string path)
        {
            using (Bitmap bmp = new Bitmap(56, 56))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                PointF[] body = { new PointF(28, 50), new PointF(5, 10), new PointF(28, 20), new PointF(51, 10) };
                g.FillPolygon(Brushes.MediumPurple, body);
                g.DrawPolygon(Pens.White, body);
                g.FillEllipse(Brushes.Yellow, 24, 25, 8, 8);
                bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private static void CreateBossSprite(string path)
        {
            using (Bitmap bmp = new Bitmap(200, 150))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Main Hull
                Rectangle hull = new Rectangle(20, 20, 160, 80);
                using (LinearGradientBrush b = new LinearGradientBrush(hull, Color.DarkRed, Color.Black, 90f))
                    g.FillEllipse(b, hull);
                
                g.DrawEllipse(new Pen(Color.Red, 4), hull);
                
                // Turrets
                g.FillRectangle(Brushes.Gray, 40, 80, 20, 30);
                g.FillRectangle(Brushes.Gray, 140, 80, 20, 30);
                
                // Core
                g.FillEllipse(Brushes.OrangeRed, 85, 40, 30, 30);
                g.DrawEllipse(new Pen(Color.Yellow, 2), 85, 40, 30, 30);
                
                bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
