using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace ZombieNet
{
    public class Button
    {
        public Rectangle Bounds { get; private set; }
        public string Text { get; private set; }
        public Action OnClick { get; set; }
        public bool IsHovered { get; private set; }
        public bool IsEnabled { get; set; } = true;
        
        public Button(Rectangle bounds, string text)
        {
            Bounds = bounds;
            Text = text;
        }

        public void Update(MouseState mouseState, MouseState prevMouseState)
        {
            if (!IsEnabled) return;

            Point mousePos = new Point(mouseState.X, mouseState.Y);
            IsHovered = Bounds.Contains(mousePos);

            if (IsHovered && mouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                OnClick?.Invoke();
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, SimpleFont font)
        {
            Color color = IsEnabled ? (IsHovered ? Color.LightGray : Color.White) : Color.Gray;
            spriteBatch.Draw(texture, Bounds, color);
            
            // Center text
            Vector2 size = font.MeasureString(Text, 2.0f);
            Vector2 pos = new Vector2(Bounds.X + (Bounds.Width - size.X) / 2, Bounds.Y + (Bounds.Height - size.Y) / 2);
            font.DrawString(spriteBatch, Text, pos, Color.Black, 2.0f);
        }
    }
}
