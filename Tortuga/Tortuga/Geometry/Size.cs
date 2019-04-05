using System;
using System.Collections.Generic;
using System.Text;

namespace Tortuga.Geometry
{
    public struct Size
    {
        public static readonly Size Empty = new Size();

        public int Width;
        public int Height;

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
