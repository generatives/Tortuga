using Tortuga.Graphics;
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
            var surface = _drawDevice.CreateSurface(image);
            var spriteSheet = SpriteSheet.CreateGrid(surface, 8, 8, 4, true);
            _tileMap = new Tilemap()
            {
                Layers = new TilemapLayer[]
                {
                    new TilemapLayer()
                    {
                        Tiles = new Tile[,]
                        {
                            { new Tile() { SubSurface = spriteSheet[1] }, new Tile() { SubSurface = spriteSheet[1] }, new Tile() { SubSurface = spriteSheet[1] } },
                            { new Tile() { SubSurface = spriteSheet[1] }, new Tile() { SubSurface = spriteSheet[1] }, new Tile() { SubSurface = spriteSheet[1] } },
                            { new Tile() { SubSurface = spriteSheet[1] }, new Tile() { SubSurface = spriteSheet[1] }, new Tile() { SubSurface = spriteSheet[1] } }
                        },
                        TileWidth = 8,
                        TileHeight = 8
                    }
                }
            };
        }

        public void Update()
        {
            double newElapsed = _frameTimer.Elapsed.TotalSeconds;
            _frameTimer.Restart();
            float deltaSeconds = (float)(newElapsed - previousElapsed);

            var vp = _viewport.Viewport;
            _drawDevice.Begin(_viewport.GetScalingTransform(), vp);

            TilemapRenderer.Render(_drawDevice, _tileMap);

            _drawDevice.End();

            _window.GraphicsDevice.SwapBuffers(_window.MainSwapchain);
            _window.GraphicsDevice.WaitForIdle();
        }
    }
}
