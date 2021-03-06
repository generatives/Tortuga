﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Tortuga.Geometry
{
    public struct Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Left => X;
        public int Right => X + Width;
        public int Bottom => Y;
        public int Top => Y + Height;

        public Vector2 TopLeft => new Vector2(Left, Top);
        public Vector2 TopRight => new Vector2(Right, Top);
        public Vector2 BottomLeft => new Vector2(Left, Bottom);
        public Vector2 BottomRight => new Vector2(Right, Bottom);

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Overlaps(Rectangle other)
        {
            if (TopRight.Y < other.BottomLeft.Y
                || BottomLeft.Y > other.TopRight.Y)
            {
                return false;
            }
            if (TopRight.X < other.BottomLeft.X
                || BottomLeft.X > other.TopRight.X)
            {
                return false;
            }
            return true;
        }
    }
}
