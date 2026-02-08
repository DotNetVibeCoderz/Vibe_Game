using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ParticleCombat
{
    public static class TextRenderer
    {
        private static Dictionary<char, string> fontData = new Dictionary<char, string>();

        static TextRenderer()
        {
            // Simple 5x5 font
            fontData['A'] = "0111010001111111000110001";
            fontData['B'] = "1111010001111101000111110";
            fontData['C'] = "0111010001100001000101110";
            fontData['D'] = "1111010001100011000111110";
            fontData['E'] = "1111110000111101000011111";
            fontData['F'] = "1111110000111101000010000";
            fontData['G'] = "0111010000100111000101110";
            fontData['H'] = "1000110001111111000110001";
            fontData['I'] = "0111000100001000010001110";
            fontData['J'] = "0011100010000101001001100";
            fontData['K'] = "1000110010111001001010001";
            fontData['L'] = "1000010000100001000011111";
            fontData['M'] = "1000111011101011000110001";
            fontData['N'] = "1000111001101011001110001";
            fontData['O'] = "0111010001100011000101110";
            fontData['P'] = "1111010001111101000010000";
            fontData['Q'] = "0111010001100011001001101";
            fontData['R'] = "1111010001111101001010001";
            fontData['S'] = "0111010000011100000101110";
            fontData['T'] = "1111100100001000010000100";
            fontData['U'] = "1000110001100011000101110";
            fontData['V'] = "1000110001100010101000100";
            fontData['W'] = "1000110001101011101110001";
            fontData['X'] = "1000101010001000101010001";
            fontData['Y'] = "1000101010001000010000100";
            fontData['Z'] = "1111100010001000100011111";
            
            fontData['0'] = "0111010001100011000101110";
            fontData['1'] = "0010001100001000010001110";
            fontData['2'] = "0111010001000100010011111";
            fontData['3'] = "1111000010001100001011110";
            fontData['4'] = "1001010010111110001000010";
            fontData['5'] = "1111110000111100000111110";
            fontData['6'] = "0111010000111101000101110";
            fontData['7'] = "1111100010001000010000100";
            fontData['8'] = "0111010001011101000101110";
            fontData['9'] = "0111010001011110000101110";

            fontData[' '] = "0000000000000000000000000";
            fontData['.'] = "0000000000000000000000100";
            fontData[':'] = "0000000100000000010000000";
            fontData['-'] = "0000000000111110000000000";
        }

        public static void DrawText(SpriteBatch spriteBatch, string text, Vector2 position, int scale, Color color)
        {
            text = text.ToUpper();
            int spacing = 1;
            int size = 5;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (fontData.ContainsKey(c))
                {
                    string data = fontData[c];
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            int index = x + y * size;
                            if (index < data.Length && data[index] == '1')
                            {
                                spriteBatch.Draw(Art.Pixel, 
                                    new Rectangle((int)(position.X + (x * scale)), (int)(position.Y + (y * scale)), scale, scale), 
                                    color);
                            }
                        }
                    }
                }
                position.X += (size + spacing) * scale;
            }
        }

        public static Vector2 MeasureString(string text, int scale)
        {
            return new Vector2(text.Length * (5 + 1) * scale, 5 * scale);
        }
    }
}