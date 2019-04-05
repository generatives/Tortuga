using Cyotek.Drawing.BitmapFont;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Drawing;
using Tortuga.Geometry;
using Veldrid;

namespace Tortuga.Graphics.Text
{
    public class TextRenderer
    {
        public static void DrawText(DrawDevice device, BitmapFont font, string text, Vector2 position)
        {
            DrawText(device, font, text, position, Vector2.One);
        }
        public static void DrawText(DrawDevice device, BitmapFont font, string text, Vector2 position, Vector2 scale)
        {
            char previousCharacter = ' ';
            var size = font.MeasureFont(text);

            foreach (char character in text)
            {
                switch (character)
                {
                    case '\n':
                        position = new Vector2(0, font.LineHeight);
                        break;
                    case '\r':
                        break;
                    default:
                        Character data;
                        int kerning;

                        data = font[character];
                        kerning = font.GetKerning(previousCharacter, character);

                        DrawCharacter(device, font, data, position.X + data.Offset.X + kerning, position.Y + data.Offset.Y);

                        position += new Vector2(data.XAdvance + kerning, 0);
                        break;
                }

                previousCharacter = character;
            }
        }

        private static void DrawCharacter(DrawDevice device, BitmapFont font, Character character, float x, float y)
        {
            var texture = font.Pages[character.TexturePage].Texture;
            device.Draw(texture, character.Bounds, new RectangleF(x, y, character.Bounds.Width, character.Bounds.Height), RgbaFloat.White);
        }
    }
}
