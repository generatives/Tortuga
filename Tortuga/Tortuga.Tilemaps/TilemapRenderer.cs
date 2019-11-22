using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Tortuga.Graphics.Resources;

namespace Tortuga.Tilemaps
{
    struct RenderableTile
    {
        public SubSurface SubSurface { get; set; }
    }

    public class TilemapRenderer
    {
        private Dictionary<TilemapLayer, RenderableTile[,]> _renderableTileLayers;
        private Dictionary<Tileset, Surface> _surfaces;
        private Tilemap _tilemap;
        private DrawDevice _drawDevice;

        public TilemapRenderer(DrawDevice drawDevice, Tilemap tilemap)
        {
            _surfaces = tilemap.Tilesets.ToDictionary(ts => ts, ts => drawDevice.CreateSurface(ts.Image));
            _renderableTileLayers = tilemap.Layers
                .ToDictionary(
                    layer => layer,
                    layer =>
                    {
                        var renderableTiles = new RenderableTile[layer.Width, layer.Height];
                        for (int x = 0; x < layer.Width; x++)
                            for (int y = 0; y < layer.Height; y++)
                            {
                                var tile = layer.Tiles[x, y];
                                renderableTiles[x, y] = new RenderableTile()
                                {
                                    SubSurface = new SubSurface(_surfaces[tile.Tileset], tile.SourceRectangle)
                                };
                            }
                        return renderableTiles;
                    });
            _tilemap = tilemap;
            _drawDevice = drawDevice;
        }

        public void Render()
        {
            foreach (var layer in _tilemap.Layers)
            {
                var renderableLayer = _renderableTileLayers[layer];
                for (int x = 0; x < layer.Width; x++)
                    for (int y = 0; y < layer.Height; y++)
                    {
                        var xPos = x * layer.TileWidth;
                        var yPos = y * layer.TileHeight;
                        var tile = renderableLayer[x, y];
                        _drawDevice.Add(tile.SubSurface, tile.SubSurface.SourceRect.Size, Matrix3x2.CreateTranslation(xPos, yPos));
                    }
            }
        }
    }
}
