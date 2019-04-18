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
        private BitmapFont _font;
        private DrawDevice _device;
        private Surface[] _pageSurfaces;

        public TextRenderer(BitmapFont font, AssetLoader loader, DrawDevice device)
        {
            _font = font;
            _device = device;
            _pageSurfaces = new Surface[_font.Pages.Length];
            for(var i = 0; i < _font.Pages.Length; i++)
            {
                var page = _font.Pages[i];
                var image = loader.LoadImage(page.FileName);
                _pageSurfaces[i] = _device.CreateSurface(image);
            }
        }

        public void DrawText(string text, Vector2 position)
        {
            DrawText(text, position, Vector2.One);
        }
        public void DrawText(string text, Vector2 position, Vector2 scale)
        {
            char previousCharacter = ' ';
            var size = _font.MeasureFont(text);

            foreach (char character in text)
            {
                switch (character)
                {
                    case '\n':
                        position = new Vector2(0, _font.LineHeight);
                        break;
                    case '\r':
                        break;
                    default:
                        Character data;
                        int kerning;

                        data = _font[character];
                        kerning = _font.GetKerning(previousCharacter, character);

                        DrawCharacter(data, position.X + data.Offset.X + kerning, position.Y + data.Offset.Y);

                        position += new Vector2(data.XAdvance + kerning, 0);
                        break;
                }

                previousCharacter = character;
            }
        }

        private void DrawCharacter(Character character, float x, float y)
        {
            var surface = _pageSurfaces[character.TexturePage];
            _device.Add(surface, character.Bounds, new RectangleF(x, y, character.Bounds.Width, character.Bounds.Height), RgbaFloat.White);
        }
    }
}
