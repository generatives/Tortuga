using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Geometry;
using System.Linq;

namespace Tortuga.Graphics.Resources
{
    public class SpriteSheet
    {
        public Surface Surface { get; private set; }
        private SubSurface[] _subSurfaces;
        public int NumSprites { get; private set; }

        public SubSurface this[int index] => _subSurfaces[index];

        public SpriteSheet(Surface surface, IEnumerable<RectangleF> rectangles)
        {
            Surface = surface;
            _subSurfaces = rectangles.Select(r => new SubSurface(surface, r)).ToArray();
            NumSprites = _subSurfaces.Length;
        }

        public static SpriteSheet CreateGrid(Surface surface, int columnWidth, int rowHeight, int numSprites, bool horizontalFirst)
        {
            var numColumns = (int)surface.Width / columnWidth;
            var numRows = (int)surface.Height / rowHeight;

            var rects = Enumerable.Range(0, numSprites).Select(i =>
            {
                var xi = horizontalFirst ? i % numColumns : i / numColumns;
                var yi = horizontalFirst ? i / numRows : i % numRows;
                return new RectangleF()
                {
                    X = xi * columnWidth,
                    Y = yi * rowHeight,
                    Width = columnWidth,
                    Height = rowHeight
                };
            });

            return new SpriteSheet(surface, rects);
        }
    }
}
