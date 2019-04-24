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

        public void DrawText(string text, Vector2 position)
        {
            DrawText(text, position, Vector2.One, RgbaFloat.White);
        }

        public void DrawText(string text, Vector2 position, RgbaFloat color)
        {
            DrawText(text, position, Vector2.One, color);
        }

        public void DrawText(string text, Vector2 position, Vector2 scale)
        {
            DrawText(text, position, scale, RgbaFloat.White);
        }

        public void DrawText(string text, Vector2 position, Vector2 scale, RgbaFloat color)
        {
            char previousCharacter = ' ';

            foreach (char character in text)
            {
                switch (character)
                {
                    case '\n':
                        position = new Vector2(0, Font.LineHeight);
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
