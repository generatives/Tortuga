using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;
using Tortuga.Geometry;

namespace Tortuga.Graphics.Resources
{
    public class GridSpriteSheet
    {
        public Texture Texture { get; private set; }
        public int ColumnWidth { get; private set; }
        public int RowHeight { get; private set; }
        public int NumColumns { get; private set; }
        public int NumRows { get; private set; }

        public Rectangle this[int index]
        {
            get
            {
                int x = index % NumColumns;
                int y = index / NumColumns;

                return this[x, y];
            }
        }

        public Rectangle this[int x, int y]
        {
            get
            {
                return new Rectangle()
                {
                    X = ColumnWidth * x,
                    Y = RowHeight * y,
                    Width = ColumnWidth,
                    Height = RowHeight
                };
            }
        }

        public GridSpriteSheet(Texture texture, int columnWidth, int rowHeight)
        {
            Texture = texture;
            ColumnWidth = columnWidth;
            RowHeight = rowHeight;
            NumColumns = (int)Texture.Width / columnWidth;
            NumRows = (int)Texture.Height / rowHeight;
        }
    }
}
