using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Tortuga.Geometry
{
    public struct RectangleF
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        public float Left => X;
        public float Right => X + Width;
        public float Bottom => Y;
        public float Top => Y + Height;

        public Vector2 TopLeft => new Vector2(Left, Top);
        public Vector2 TopRight => new Vector2(Right, Top);
        public Vector2 BottomLeft => new Vector2(Left, Bottom);
        public Vector2 BottomRight => new Vector2(Right, Bottom);

        public Vector2 Size => new Vector2(Width, Height);
        public Vector2 Position => new Vector2(X, Y);

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public static RectangleF Square(float size)
        {
            return new RectangleF(0, 0, size, size);
        }

        public bool Overlaps(RectangleF other)
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
