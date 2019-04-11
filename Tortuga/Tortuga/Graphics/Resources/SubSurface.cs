using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Geometry;

namespace Tortuga.Graphics.Resources
{
    public class SubSurface
    {
        public Surface Surface { get; private set; }
        public RectangleF SourceRect { get; private set; }

        public SubSurface(Surface surface, RectangleF sourceRect)
        {
            Surface = surface;
            SourceRect = sourceRect;
        }
    }
}
