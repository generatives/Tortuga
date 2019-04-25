using Cyotek.Drawing.BitmapFont;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Tortuga.Geometry;
using Tortuga.Graphics.Resources;
using Veldrid;
using Veldrid.ImageSharp;
using Tortuga.Assets;

namespace Tortuga.Graphics.Text
{
    [Flags]
    public enum TextAlignment
    {
        BOTTOM = 0,
        TOP = 1,
        MIDDLE = 2,
        LEFT = 0,
        RIGHT = 4,
        CENTER = 8,
        TOP_MIDDLE = TOP | MIDDLE,
        TOP_LEFT = TOP | LEFT,
        TOP_RIGHT = TOP | RIGHT,
        CENTER_MIDDLE = CENTER | MIDDLE,
        CENTER_LEFT = CENTER | LEFT,
        CENTER_RIGHT = CENTER | RIGHT,
        BOTTOM_MIDDLE = BOTTOM | MIDDLE,
        BOTTOM_LEFT = BOTTOM | LEFT,
        BOTTOM_RIGHT = BOTTOM | RIGHT
    }

    public class TextRenderer
    {
        public BitmapFont Font { get; private set; }
        private DrawDevice _device;
        private Surface[] _pageSurfaces;

        public TextRenderer(BitmapFont font, AssetLoader loader, DrawDevice device)
        {
            Font = font;
            _device = device;
            _pageSurfaces = new Surface[Font.Pages.Length];
            for(var i = 0; i < Font.Pages.Length; i++)
            {
                var page = Font.Pages[i];
                var image = loader.LoadImage(page.FileName);
                _pageSurfaces[i] = _device.CreateSurface(image);
            }
        }

        private bool TextAlignemntContains(TextAlignment flags, TextAlignment test)
        {
            return (flags & test) == test;
        }

        public void DrawText(string text, Vector2 position, Vector2? scaleParam = null, RgbaFloat? colorParam = null, TextAlignment textAlignment = TextAlignment.BOTTOM_LEFT)
        {
            var scale = scaleParam.HasValue ? scaleParam.Value : Vector2.One;
            var color = colorParam.HasValue ? colorParam.Value : RgbaFloat.White;

            char previousCharacter = ' ';
            var size = Font.MeasureFont(text);
            if(TextAlignemntContains(textAlignment, TextAlignment.MIDDLE))
            {
                position -= new Vector2(size.Width / 2f, 0);
            }
            else if(TextAlignemntContains(textAlignment, TextAlignment.RIGHT))
            {
                position -= new Vector2(size.Width, 0);
            }

            if (TextAlignemntContains(textAlignment, TextAlignment.CENTER))
            {
                position -= new Vector2(0, size.Height / 2f);
            }
            else if (TextAlignemntContains(textAlignment, TextAlignment.TOP))
            {
                position -= new Vector2(0, size.Height);
            }

            foreach (char character in text)
            {
                switch (character)
                {
                    case '\n':
                        position += new Vector2(0, Font.LineHeight);
                        break;
                    case '\r':
                        break;
                    default:
                        Character data;
                        int kerning;

                        data = Font[character];
                        kerning = Font.GetKerning(previousCharacter, character);

                        DrawCharacter(data, position.X + data.Offset.X + kerning, position.Y + data.Offset.Y, scale, color);

                        position += new Vector2(data.XAdvance + kerning, 0);
                        break;
                }

                previousCharacter = character;
            }
        }

        private void DrawCharacter(Character character, float x, float y, Vector2 scale, RgbaFloat color)
        {
            var surface = _pageSurfaces[character.TexturePage];
            _device.Add(
                surface,
                character.Bounds,
                new RectangleF(x * scale.X, y * scale.Y, character.Bounds.Width * scale.X, character.Bounds.Height * scale.Y),
                color);
        }
    }
}
