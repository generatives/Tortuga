using Tortuga.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Tortuga.Drawing.Resources
{
    public class SubTexture
    {
        public RectangleF TexRect { get; private set; }
        public Rectangle TexRectPix { get; private set; }
        public Texture Texture { get; private set; }

        public Vector2 Size
        {
            get
            {
                return new Vector2(
                    Texture.Width * TexRect.Width,
                    Texture.Height * TexRect.Height);
            }
        }

        public SubTexture(Texture texture)
        {
            Texture = texture;
            TexRect = new RectangleF(0, 0, 1, 1);
        }

        public SubTexture(Texture texture, RectangleF textRect)
        {
            Texture = texture;
            TexRect = textRect;
            TexRectPix = new Rectangle(
                (int)(textRect.X * texture.Width),
                (int)(textRect.Y * texture.Height),
                (int)(textRect.Width * texture.Width),
                (int)(textRect.Height * texture.Height));
        }

        public SubTexture(Texture texture, Rectangle textRectPix)
        {
            Texture = texture;
            TexRectPix = textRectPix;
            TexRect = new RectangleF(
                (float)textRectPix.X / texture.Width,
                (float)textRectPix.Y / texture.Height,
                (float)textRectPix.Width / texture.Width,
                (float)textRectPix.Height / texture.Height);
        }

        public static implicit operator SubTexture(Texture text)
        {
            return new SubTexture(text);
        }
    }
}
