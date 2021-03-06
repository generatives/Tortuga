﻿using Tortuga.Graphics;
using Tortuga.Geometry;
using Tortuga.DesktopPlatform;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.StartupUtilities;
using Tortuga.Platform;
using SixLabors.ImageSharp;
using Veldrid.ImageSharp;
using Tortuga.Graphics.Text;
using Cyotek.Drawing.BitmapFont;
using Tortuga.Assets;
using Tortuga.Graphics.Resources;
using Tortuga.Tilemaps;

namespace OpenSkiesDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            game.Run();
        }
    }

    class Game
    {
        private IWindow _window;
        private AssetLoader _assetLoader;
        private double previousElapsed;
        private BitmapFont font;
        private TextRenderer textRenderer;
        private Tilemap _tileMap;
        private TilemapRenderer _tilemapRenderer;
        private Stopwatch _frameTimer;
        private DrawDevice _drawDevice;
        private ViewportManager _viewport;

        public void Run()
        {
            var platform = new DesktopPlatform();
            _assetLoader = AssetLoader.DefaultAssetLoader(platform);

            WindowCreateInfo wci = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowTitle = "Tortuga Demo"
            };

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true);
#if DEBUG
            options.Debug = true;
#endif

            _window = platform.CreateWindow(wci, options);
            _window.GraphicsDeviceCreated += LoadResources;
            _window.Tick += Update;
            _window.Resized += _window_Resized;

            _viewport = new ViewportManager(1280, 720);

            _frameTimer = Stopwatch.StartNew();
            double previousElapsed = _frameTimer.Elapsed.TotalSeconds;

            var task = _window.Run();
            Task.WaitAll(task);
        }

        private void _window_Resized()
        {
            _viewport.WindowChanged(_window.Width, _window.Height);
        }

        public void LoadResources()
        {
            _drawDevice = new DrawDevice(_window.GraphicsDevice, _window.MainSwapchain);

            font = _assetLoader.LoadFont(BitmapFont.DefaultFontName);
            textRenderer = new TextRenderer(font, _assetLoader, _drawDevice);

            var image = _assetLoader.LoadImage("Bird.png");
            var tileset = new Tileset()
            {
                Image = image
            };
            var tile1 = new Tile() { Tileset = tileset, SourceRectangle = new RectangleF(0, 0, 8, 8) };
            var tile2 = new Tile() { Tileset = tileset, SourceRectangle = new RectangleF(0, 8, 8, 8) };
            _tileMap = new Tilemap()
            {
                Layers = new TilemapLayer[]
                {
                    new TilemapLayer()
                    {
                        Tiles = new Tile[,]
                        {
                            { tile2, tile1, tile2 },
                            { tile1, tile2, tile1 },
                            { tile2, tile1, tile2 }
                        },
                        TileWidth = 8,
                        TileHeight = 8
                    }
                },
                Tilesets = new Tileset[]
                {
                    tileset
                }
            };
            _tilemapRenderer = new TilemapRenderer(_drawDevice, _tileMap);
        }

        public void Update()
        {
            double newElapsed = _frameTimer.Elapsed.TotalSeconds;
            _frameTimer.Restart();
            float deltaSeconds = (float)(newElapsed - previousElapsed);

            var vp = _viewport.Viewport;
            _drawDevice.Begin(_viewport.GetScalingTransform() * Matrix4x4.CreateScale(5), vp);

            _tilemapRenderer.Render();

            _drawDevice.End();

            _window.GraphicsDevice.SwapBuffers(_window.MainSwapchain);
            _window.GraphicsDevice.WaitForIdle();
        }
    }
}
