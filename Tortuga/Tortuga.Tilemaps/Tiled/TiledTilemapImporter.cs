using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TiledSharp;
using Tortuga.Assets.Importers;
using Tortuga.Geometry;
using Tortuga.Graphics.Resources;

namespace Tortuga.Tilemaps.Tiled
{
    public class TiledTilemapImporter : IImporter
    {
        public object Import(Stream stream)
        {
            var map = new TmxMap(stream);
            var tMapWidth = map.Width;
            var tMapHeight = map.Height;
            var tileRect = new Dictionary<int, RectangleF>();
            var idSet = new Dictionary<int, Tileset>();
            var layerID = new List<int[,]>();

            foreach (TmxTileset ts in map.Tilesets)
            {
                var image = Image.Load(ts.Image.Data);
                var tileset = new Tileset()
                {
                    Image = image
                };

                // Loop hoisting
                var wStart = ts.Margin;
                var wInc = ts.TileWidth + ts.Spacing;
                var wEnd = image.Width;

                var hStart = ts.Margin;
                var hInc = ts.TileHeight + ts.Spacing;
                var hEnd = image.Height;

                // Pre-compute tileset rectangles
                var id = ts.FirstGid;
                for (var h = hStart; h < hEnd; h += hInc)
                {
                    for (var w = wStart; w < wEnd; w += wInc)
                    {
                        var rect = new RectangleF(w, h, ts.TileWidth, ts.TileHeight);
                        idSet.Add(id, tileset);
                        tileRect.Add(id, rect);
                        id += 1;
                    }
                }
            }

            var layers = map.Layers
                .Select(layer =>
                {
                    var tiles = new Tile[map.Width, map.Height];
                    foreach (TmxLayerTile t in layer.Tiles)
                    {
                        tiles[t.X, t.Y] = new Tile()
                        {
                            Tileset = idSet[t.Gid],
                            SourceRectangle = tileRect[t.Gid]
                        };
                    }
                    return new TilemapLayer()
                    {
                        Tiles = tiles,
                        TileWidth = map.Width,
                        TileHeight = map.Height
                    };
                })
                .ToArray();

            var tilemap = new Tilemap()
            {
                Layers = layers
            };

            return tilemap;
        }
    }
}
