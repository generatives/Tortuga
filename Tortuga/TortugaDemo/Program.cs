using Tortuga.Drawing;
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
using Tortuga.Graphics;

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
        private Vector2 objectPosition;
        private IWindow _window;
        private double previousElapsed;
        private BitmapFont font;
        private TextRenderer textRenderer;
        private Stopwatch sw;
        private DrawDevice drawDevice;
        private ViewportManager viewport;
        private Vertex[] triangleVertices = new Vertex[]
            {
                new Vertex(new Vector2(0, 0), RgbaFloat.White, new Vector2(0, 1)),
                new Vertex(new Vector2(10, 20), RgbaFloat.White, new Vector2(0.5f, 0)),
                new Vertex(new Vector2(20, 0), RgbaFloat.White, new Vector2(1, 1))
            };

        public void Run()
        {
            var platform = new DesktopPlatform();

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

            viewport = new ViewportManager(500, 500);
            viewport.WindowChanged(1280, 720);

            _window = platform.CreateWindow(wci, options);
            _window.GraphicsDeviceCreated += LoadResources;
            _window.Tick += Update;
            _window.Resized += _window_Resized;

            sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            var task = _window.Run();
            Task.WaitAll(task);
        }

        private void _window_Resized()
        {
            viewport.WindowChanged(_window.Width, _window.Height);
        }

        public void LoadResources()
        {
            drawDevice = new DrawDevice(_window.GraphicsDevice, _window.MainSwapchain);

            font = new BitmapFont();
            font.Load("Assets/testfont.fnt");
            textRenderer = new TextRenderer(font, drawDevice);
        }

        public void Update()
        {
            double newElapsed = sw.Elapsed.TotalSeconds;
            sw.Restart();
            float deltaSeconds = (float)(newElapsed - previousElapsed);

            var change = Vector2.Zero;
            if (_window.InputTracker.GetKey(TKey.A))
            {
                change += new Vector2(-1, 0);
            }

            if (_window.InputTracker.GetKey(TKey.D))
            {
                change += new Vector2(1, 0);
            }

            if (_window.InputTracker.GetKey(TKey.W))
            {
                change += new Vector2(0, 1);
            }

            if (_window.InputTracker.GetKey(TKey.S))
            {
                change += new Vector2(0, -1);
            }

            objectPosition += change;

            //if (InputTracker.GetKey(Key.E))
            //{
            //    _camera.GameObject.Transform.Rotation += 0.3f;
            //}

            //if (InputTracker.GetKey(Key.Q))
            //{
            //    _camera.GameObject.Transform.Rotation += -0.3f;
            //}

            //if (InputTracker.GetKey(Key.Z))
            //{
            //    _camera.Zoom += new Vector2(0.05f, 0.05f);
            //}

            //if (InputTracker.GetKey(Key.X))
            //{
            //    _camera.Zoom += new Vector2(-0.05f, -0.05f);
            //}

            var vp = viewport.Viewport;
            drawDevice.Begin(viewport.GetScalingTransform() * Matrix4x4.CreateScale(1f / vp.Width, 1f / vp.Height, 1f), vp);
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1), new RectangleF(-250f, -250f, 500f, 500f), RgbaFloat.Black);
            drawDevice.Add(drawDevice.Grid, triangleVertices);
            drawDevice.Add(drawDevice.Grid, triangleVertices, Matrix3x2.CreateRotation(0.5f) * Matrix3x2.CreateTranslation(new Vector2(50, 50)));
            drawDevice.Add(drawDevice.Grid, RectangleF.Square(1), new RectangleF(-50, 0, 30f, 30f), RgbaFloat.Red);
            drawDevice.Add(drawDevice.Grid, RectangleF.Square(1), new Vector2(30f, 30f), Matrix3x2.CreateTranslation(100, 0), RgbaFloat.Yellow);

            textRenderer.DrawText("Rendering Text!", new Vector2(-250, 30), new Vector2(5, 5));
            drawDevice.End();
        }
    }
}
