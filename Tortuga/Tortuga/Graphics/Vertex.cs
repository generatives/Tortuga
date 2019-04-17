using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Tortuga.Graphics
{
    public struct Vertex
    {
        public const uint SizeInBytes = 32;
        public Vector2 Position;
        public RgbaFloat Color;
        public Vector2 UV;

        public Vertex(Vector2 position, RgbaFloat color, Vector2 uv)
        {
            Position = position;
            Color = color;
            UV = uv;
        }
    }
}
