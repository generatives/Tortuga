using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Drawing;

namespace Tortuga.Graphics.Text
{
    public class TextRenderer
    {
        public static void DrawText(DrawDevice device, Font font, string text, Vector2 position)
        {
            var size = new Vector2(font.CharacterWidth, font.CharacterHeight);
            foreach(var c in text)
            {
                if(c != ' ')
                {
                    var texture = font[c];
                    device.Draw(texture, size, Matrix3x2.CreateScale(5) * Matrix3x2.CreateTranslation(position));
                }
                position = new Vector2(position.X + font.CharacterWidth * 5, position.Y);
            }
        }
    }
}
