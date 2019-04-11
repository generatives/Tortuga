using Android.App;
using Android.Content.PM;
using Android.OS;
using Tortuga.Drawing;
using Tortuga.AndroidPlatform;
using Tortuga.Audio;
using Tortuga.Platform;
using System.Numerics;
using System.Threading;
using Veldrid;
using System;
using Tortuga.Geometry;

namespace OpenSkiesAndroidDemo
{
    [Activity(
        MainLauncher = true,
        Label = "TortugaDemo",
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize
        )]
    public class MainActivity : Activity
    {
        private AndroidPlatform _platform;
        private IWindow _window;
        private DrawDevice _drawDevice;
        private Vector2 objectPosition = Vector2.Zero;
        private Vertex[] triangleVertices = new Vertex[]
            {
                new Vertex(new Vector2(0, 0), RgbaFloat.White, new Vector2(0, 1)),
                new Vertex(new Vector2(10, 20), RgbaFloat.White, new Vector2(0.5f, 0)),
                new Vertex(new Vector2(20, 0), RgbaFloat.White, new Vector2(1, 1))
            };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            bool debug = false;
#if DEBUG
            debug = true;
#endif

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug,
                PixelFormat.R16_UNorm,
                false,
                ResourceBindingModel.Improved,
                true,
                true);

            _platform = new AndroidPlatform(this);
            _window = _platform.CreateWindow(options);
            _window.Tick += _window_Tick;
            _window.Run();
        }

        private float rotation;
        private ISound sound;

        private void _window_Tick()
        {
            if (_drawDevice == null)
            {
                _drawDevice = new DrawDevice(_window.GraphicsDevice, _window.MainSwapchain);
            }

            //if (sound == null)
            //{
            //    sound = _platform.CreateSound("raw/song");
            //    sound.Play();
            //}

            rotation += 0.02f;
            _drawDevice.Begin(Matrix4x4.CreateScale(1f / _window.Width, 1f / _window.Height, 1f));
            _drawDevice.Add(_drawDevice.Grid, triangleVertices);
            _drawDevice.Add(_drawDevice.Grid, triangleVertices, Matrix3x2.CreateRotation(0.5f) * Matrix3x2.CreateTranslation(new Vector2(50, 50)));
            _drawDevice.Add(_drawDevice.Grid, RectangleF.Square(1), new RectangleF(-50, 0, 30f, 30f), RgbaFloat.CornflowerBlue);
            _drawDevice.Add(_drawDevice.Grid, RectangleF.Square(1), new Vector2(30f, 30f), Matrix3x2.CreateTranslation(100, 0), RgbaFloat.CornflowerBlue);
            _drawDevice.End();
        }

        protected override void OnPause()
        {
            base.OnPause();
            _window.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            _window.Resume();
        }
    }
}