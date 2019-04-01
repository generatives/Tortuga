using OpenSkies.Drawing;
using OpenSkies.Geometry;
using Tortuga.DesktopPlatform;
using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.StartupUtilities;

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

        public void Run()
        {
            var platform = new DesktopPlatform();

            WindowCreateInfo wci = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowTitle = "OpenSkies Demo"
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

            var _window = platform.CreateWindow(wci, options);
            DrawDevice drawDevice = null;
            _window.GraphicsDeviceCreated += () => drawDevice = new DrawDevice(_window.GraphicsDevice, _window.MainSwapchain);

            Stopwatch sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;

            var triangleVertices = new Vertex[]
            {
                new Vertex(new Vector2(0, 0), RgbaFloat.Orange, new Vector2(0, 0)),
                new Vertex(new Vector2(10, 20), RgbaFloat.Blue, new Vector2(0.5f, 1)),
                new Vertex(new Vector2(20, 0), RgbaFloat.Green, new Vector2(1, 0))
            };

            _window.Tick += () =>
            {
                double newElapsed = sw.Elapsed.TotalSeconds;
                sw.Restart();
                float deltaSeconds = (float)(newElapsed - previousElapsed);

                InputSnapshot inputSnapshot = (_window as DesktopWindow).Window.PumpEvents();
                InputTracker.UpdateFrameInput(inputSnapshot);

                Update(deltaSeconds);

                drawDevice.Begin(Matrix4x4.CreateScale(1f / _window.Width, 1f / _window.Height, 1f));
                drawDevice.Draw(drawDevice.WhitePixel, new RectangleF(objectPosition.X - 15f, objectPosition.Y - 15f, 30f, 30f), RgbaFloat.White);
                drawDevice.Draw(drawDevice.Grid, new RectangleF(objectPosition.X - 50f, objectPosition.Y, 30f, 30f), RgbaFloat.Red);
                drawDevice.Draw(drawDevice.Grid, new RectangleF(objectPosition.X + 50f, objectPosition.Y, 30f, 30f));
                drawDevice.Draw(drawDevice.Grid, triangleVertices);
                drawDevice.Draw(drawDevice.WhitePixel, triangleVertices, Matrix3x2.CreateRotation(1) * Matrix3x2.CreateTranslation(new Vector2(-50, -50)));
                drawDevice.Draw(drawDevice.Grid, new Vector2(30f, 30f), Matrix3x2.CreateTranslation(50, 0));
                drawDevice.Draw(drawDevice.Grid, new Vector2(30f, 30f), RgbaFloat.CornflowerBlue, Matrix3x2.CreateTranslation(100, 0));
                drawDevice.Draw(drawDevice.WhitePixel, new Vector2(40f, 40f), new Vector2(-100, 0), -1);
                drawDevice.Draw(drawDevice.WhitePixel, new Vector2(40f, 40f), new Vector2(-100, 100), -0.5f, RgbaFloat.Grey);
                drawDevice.End();
            };

            var task = _window.Run();
            Task.WaitAll(task);
        }

        public void Update(float time)
        {
            var change = Vector2.Zero;
            if (InputTracker.GetKey(Key.A))
            {
                change += new Vector2(-1, 0);
            }

            if (InputTracker.GetKey(Key.D))
            {
                change += new Vector2(1, 0);
            }

            if (InputTracker.GetKey(Key.W))
            {
                change += new Vector2(0, 1);
            }

            if (InputTracker.GetKey(Key.S))
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
        }
    }
}
