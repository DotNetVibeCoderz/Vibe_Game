using System;
using System.Drawing;

namespace Tamagochi
{
    public static class PixelRenderer
    {
        private static readonly int[,] DinoSprite = new int[,]
        {
            {0,0,0,0,0,1,1,1,0,0,0,0},
            {0,0,0,0,1,1,1,1,1,0,0,0},
            {0,0,0,0,1,2,1,1,0,0,0,0},
            {0,0,0,0,1,1,1,1,0,0,0,0},
            {0,1,1,0,1,1,1,1,0,0,0,0},
            {1,1,1,1,1,1,1,1,1,1,0,0},
            {1,1,1,1,1,1,1,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,1,1,0,0},
            {0,0,0,1,0,0,1,0,0,1,0,0},
            {0,0,0,1,1,0,1,1,0,1,1,0}
        };

        private static readonly int[,] CatSprite = new int[,]
        {
            {0,0,1,0,0,0,0,0,1,0,0,0},
            {0,1,1,1,0,0,0,1,1,1,0,0},
            {0,1,1,1,1,1,1,1,1,1,0,0},
            {0,1,2,1,1,1,1,1,2,1,0,0},
            {0,1,1,1,1,1,1,1,1,1,0,0},
            {0,0,1,1,1,1,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0,0},
            {0,0,1,0,0,0,0,0,1,0,0,0},
            {0,0,1,0,0,0,0,0,1,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0}
        };
        
        private static readonly int[,] RobotSprite = new int[,]
        {
            {0,0,0,1,1,1,1,1,0,0,0,0},
            {0,0,0,1,2,1,2,1,0,0,0,0},
            {0,0,0,1,1,1,1,1,0,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0,0},
            {0,1,0,1,1,1,1,1,0,1,0,0},
            {0,1,0,1,1,1,1,1,0,1,0,0},
            {0,0,0,1,1,1,1,1,0,0,0,0},
            {0,0,0,1,0,0,0,1,0,0,0,0},
            {0,0,0,1,0,0,0,1,0,0,0,0},
            {0,0,1,1,0,0,0,1,1,0,0,0}
        };

        private static readonly int[,] EggSprite = new int[,]
        {
            {0,0,0,0,1,1,1,0,0,0,0,0},
            {0,0,0,1,1,1,1,1,0,0,0,0},
            {0,0,0,1,1,1,1,1,0,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0,0},
            {0,0,0,1,1,1,1,1,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0}
        };
        
        private static readonly int[,] DeadSprite = new int[,]
        {
            {0,0,0,0,0,1,0,0,0,0,0,0},
            {0,0,0,0,1,1,1,0,0,0,0,0},
            {0,0,0,1,1,1,1,1,0,0,0,0},
            {0,0,0,1,2,1,2,1,0,0,0,0},
            {0,0,0,1,1,1,1,1,0,0,0,0},
            {0,0,0,0,1,1,1,0,0,0,0,0},
            {0,0,1,1,1,1,1,1,1,0,0,0},
            {0,0,1,0,0,0,0,0,1,0,0,0},
            {0,0,1,0,0,0,0,0,1,0,0,0},
            {0,0,0,0,0,0,0,0,0,0,0,0}
        };

        public static Bitmap DrawPet(PetModel pet, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent); // Background handled by form

                int[,] sprite = EggSprite;
                Color mainColor = Color.White;

                if (pet.Stage == LifecycleStage.Dead)
                {
                    sprite = DeadSprite;
                    mainColor = Color.Gray;
                }
                else if (pet.Stage == LifecycleStage.Egg)
                {
                    sprite = EggSprite;
                    mainColor = Color.LightYellow;
                }
                else
                {
                    switch (pet.Type)
                    {
                        case PetType.Dino:
                            sprite = DinoSprite;
                            mainColor = Color.LimeGreen;
                            break;
                        case PetType.Cat:
                            sprite = CatSprite;
                            mainColor = Color.Orange;
                            break;
                        case PetType.Robot:
                            sprite = RobotSprite;
                            mainColor = Color.Cyan;
                            break;
                    }
                }

                int scale = 20; // Size of each "pixel"
                if (pet.Stage == LifecycleStage.Baby) scale = 12;

                int offsetX = (width - (12 * scale)) / 2;
                int offsetY = (height - (10 * scale)) / 2;
                
                // Add simple bobbing animation
                if (pet.Stage != LifecycleStage.Dead && pet.Stage != LifecycleStage.Egg)
                {
                    int bob = (DateTime.Now.Millisecond / 500) % 2 == 0 ? 0 : 5;
                    offsetY += bob;
                }
                else if (pet.Stage == LifecycleStage.Egg)
                {
                     // Shake animation
                     int shake = (DateTime.Now.Millisecond / 100) % 3; 
                     offsetX += (shake - 1) * 2;
                }

                for (int y = 0; y < 10; y++)
                {
                    for (int x = 0; x < 12; x++)
                    {
                        int pixel = sprite[y, x];
                        if (pixel == 0) continue;

                        Color color = pixel == 1 ? mainColor : Color.Black; // 1 = Body, 2 = Eyes
                        
                        // Eye color logic
                        if (pixel == 2)
                        {
                            if (pet.CurrentEmotion == Emotion.Angry) color = Color.Red;
                            else if (pet.CurrentEmotion == Emotion.Sick) color = Color.GreenYellow;
                            else if (pet.CurrentEmotion == Emotion.Sad) color = Color.Blue;
                            else color = Color.Black;
                        }

                        using (Brush b = new SolidBrush(color))
                        {
                            g.FillRectangle(b, offsetX + (x * scale), offsetY + (y * scale), scale, scale);
                        }
                    }
                }
                
                // Draw Status Emote Bubble
                if (pet.Stage != LifecycleStage.Egg && pet.Stage != LifecycleStage.Dead)
                {
                    DrawBubble(g, pet.CurrentEmotion, width - 60, 20);
                }
            }
            return bmp;
        }
        
        private static void DrawBubble(Graphics g, Emotion emotion, int x, int y)
        {
            string icon = "😐";
            switch(emotion)
            {
                case Emotion.Happy: icon = "😄"; break;
                case Emotion.Angry: icon = "😡"; break;
                case Emotion.Sad: icon = "😢"; break;
                case Emotion.Sick: icon = "🤢"; break;
            }
            
            using (Font f = new Font("Segoe UI Emoji", 24))
            {
                g.DrawString(icon, f, Brushes.Black, x, y);
            }
        }
    }
}
