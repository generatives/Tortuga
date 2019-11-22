using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Geometry;
using Tortuga.Graphics.Resources;

namespace Tortuga.Tilemaps
{
    public struct Tile
    {
        public Tileset Tileset { get; set; }
        public RectangleF SourceRectangle { get; set; }
    }
}
