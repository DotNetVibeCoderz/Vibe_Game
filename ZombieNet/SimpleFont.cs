using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ZombieNet
{
    public class SimpleFont
    {
        private Texture2D _texture;
        private Dictionary<char, Rectangle> _glyphs;
        private int _charWidth = 5;
        private int _charHeight = 7;
        private int _spacing = 1;

        public SimpleFont(GraphicsDevice graphicsDevice)
        {
            GenerateFont(graphicsDevice);
        }

        private void GenerateFont(GraphicsDevice graphicsDevice)
        {
            // 5x7 bitmap font definitions (0 = transparent, 1 = white)
            // Each byte represents a column, 7 bits used.
            var patterns = new Dictionary<char, byte[]>
            {
                {'A', new byte[]{0x7E, 0x09, 0x09, 0x09, 0x7E}},
                {'B', new byte[]{0x7F, 0x49, 0x49, 0x49, 0x36}},
                {'C', new byte[]{0x3E, 0x41, 0x41, 0x41, 0x22}},
                {'D', new byte[]{0x7F, 0x41, 0x41, 0x22, 0x1C}},
                {'E', new byte[]{0x7F, 0x49, 0x49, 0x49, 0x41}},
                {'F', new byte[]{0x7F, 0x09, 0x09, 0x09, 0x01}},
                {'G', new byte[]{0x3E, 0x41, 0x49, 0x49, 0x7A}},
                {'H', new byte[]{0x7F, 0x08, 0x08, 0x08, 0x7F}},
                {'I', new byte[]{0x00, 0x41, 0x7F, 0x41, 0x00}},
                {'J', new byte[]{0x20, 0x40, 0x41, 0x3F, 0x01}},
                {'K', new byte[]{0x7F, 0x08, 0x14, 0x22, 0x41}},
                {'L', new byte[]{0x7F, 0x40, 0x40, 0x40, 0x40}},
                {'M', new byte[]{0x7F, 0x02, 0x0C, 0x02, 0x7F}},
                {'N', new byte[]{0x7F, 0x04, 0x08, 0x10, 0x7F}},
                {'O', new byte[]{0x3E, 0x41, 0x41, 0x41, 0x3E}},
                {'P', new byte[]{0x7F, 0x09, 0x09, 0x09, 0x06}},
                {'Q', new byte[]{0x3E, 0x41, 0x51, 0x21, 0x5E}},
                {'R', new byte[]{0x7F, 0x09, 0x19, 0x29, 0x46}},
                {'S', new byte[]{0x46, 0x49, 0x49, 0x49, 0x31}},
                {'T', new byte[]{0x01, 0x01, 0x7F, 0x01, 0x01}},
                {'U', new byte[]{0x3F, 0x40, 0x40, 0x40, 0x3F}},
                {'V', new byte[]{0x1F, 0x20, 0x40, 0x20, 0x1F}},
                {'W', new byte[]{0x3F, 0x40, 0x38, 0x40, 0x3F}},
                {'X', new byte[]{0x63, 0x14, 0x08, 0x14, 0x63}},
                {'Y', new byte[]{0x07, 0x08, 0x70, 0x08, 0x07}},
                {'Z', new byte[]{0x61, 0x51, 0x49, 0x45, 0x43}},
                {'0', new byte[]{0x3E, 0x51, 0x49, 0x45, 0x3E}},
                {'1', new byte[]{0x00, 0x42, 0x7F, 0x40, 0x00}},
                {'2', new byte[]{0x42, 0x61, 0x51, 0x49, 0x46}},
                {'3', new byte[]{0x21, 0x41, 0x45, 0x4B, 0x31}},
                {'4', new byte[]{0x18, 0x14, 0x12, 0x7F, 0x10}},
                {'5', new byte[]{0x27, 0x45, 0x45, 0x45, 0x39}},
                {'6', new byte[]{0x3C, 0x4A, 0x49, 0x49, 0x30}},
                {'7', new byte[]{0x01, 0x71, 0x09, 0x05, 0x03}},
                {'8', new byte[]{0x36, 0x49, 0x49, 0x49, 0x36}},
                {'9', new byte[]{0x06, 0x49, 0x49, 0x29, 0x1E}},
                {' ', new byte[]{0x00, 0x00, 0x00, 0x00, 0x00}},
                {'-', new byte[]{0x08, 0x08, 0x08, 0x08, 0x08}},
                {'.', new byte[]{0x00, 0x60, 0x60, 0x00, 0x00}},
                {':', new byte[]{0x00, 0x36, 0x36, 0x00, 0x00}},
                {'!', new byte[]{0x00, 0x00, 0x5F, 0x00, 0x00}},
                {'|', new byte[]{0x00, 0x00, 0x7F, 0x00, 0x00}}
            };

            int width = patterns.Count * (_charWidth + 1);
            int height = _charHeight;
            Color[] data = new Color[width * height];
            _glyphs = new Dictionary<char, Rectangle>();

            int offsetX = 0;
            foreach (var kvp in patterns)
            {
                char c = kvp.Key;
                byte[] cols = kvp.Value;
                _glyphs[c] = new Rectangle(offsetX, 0, _charWidth, _charHeight);

                for (int x = 0; x < _charWidth; x++)
                {
                    byte column = cols[x];
                    for (int y = 0; y < _charHeight; y++)
                    {
                        if (((column >> y) & 1) == 1)
                        {
                            data[(offsetX + x) + y * width] = Color.White;
                        }
                    }
                }
                offsetX += _charWidth + 1;
            }

            _texture = new Texture2D(graphicsDevice, width, height);
            _texture.SetData(data);
        }

        public void DrawString(SpriteBatch spriteBatch, string text, Vector2 position, Color color, float scale = 2.0f)
        {
            if (string.IsNullOrEmpty(text)) return;
            text = text.ToUpper();
            float x = position.X;
            float y = position.Y;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    x = position.X;
                    y += (_charHeight + 2) * scale;
                    continue;
                }

                if (_glyphs.ContainsKey(c))
                {
                    spriteBatch.Draw(_texture, new Vector2(x, y), _glyphs[c], color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }
                x += (_charWidth + _spacing) * scale;
            }
        }
        
        public Vector2 MeasureString(string text, float scale = 2.0f)
        {
             if (string.IsNullOrEmpty(text)) return Vector2.Zero;
             text = text.ToUpper();
             float width = 0;
             float height = (_charHeight) * scale;
             float currentWidth = 0;
             
             foreach(char c in text)
             {
                 if (c == '\n') {
                     if (currentWidth > width) width = currentWidth;
                     currentWidth = 0;
                     height += (_charHeight + 2) * scale;
                 }
                 else {
                     currentWidth += (_charWidth + _spacing) * scale;
                 }
             }
             if (currentWidth > width) width = currentWidth;
             return new Vector2(width, height);
        }
    }
}
