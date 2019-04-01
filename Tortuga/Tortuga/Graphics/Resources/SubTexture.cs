using OpenSkies.Geometry;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace OpenSkies.Drawing.Resources
{
    public class SubTexture
    {
        public RectangleF TexRect { get; private set; }
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
        }

        public static implicit operator SubTexture(Texture text)
        {
            return new SubTexture(text);
        }
    }
}
