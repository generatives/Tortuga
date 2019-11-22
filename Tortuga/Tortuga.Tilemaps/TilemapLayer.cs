using System;
using System.Collections.Generic;
using System.Text;

namespace Tortuga.Tilemaps
{
    public class TilemapLayer
    {
        public Tile[,] Tiles { get; set; }
        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
    }
}
