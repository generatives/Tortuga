using Tortuga.Drawing.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace Tortuga.Graphics.Resources
{
    public class GridSpriteSheet
    {
        public Texture Texture { get; private set; }
        public int ColumnWidth { get; private set; }
        public int RowHeight { get; private set; }
        public int NumColumns { get; private set; }
        public int NumRows { get; private set; }

        public SubTexture this[int index]
        {
            get
            {
                int x = index % NumColumns;
                int y = index / NumColumns;

                return this[x, y];
            }
        }

        public SubTexture this[int x, int y]
        {
            get
            {
                return new SubTexture(Texture,
                    new Geometry.Rectangle()
                    {
                        X = ColumnWidth * x,
                        Y = RowHeight * y,
                        Width = ColumnWidth,
                        Height = RowHeight
                    });
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
