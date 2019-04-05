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
            if (_drawDevice == null) _drawDevice = new DrawDevice(_window.GraphicsDevice, _window.MainSwapchain);

            //if (sound == null)
            //{
            //    sound = _platform.CreateSound("raw/song");
            //    sound.Play();
            //}

            rotation += 0.02f;
            _drawDevice.Begin(Matrix4x4.CreateScale(1f / _window.Width, 1f / _window.Height, 1f));
            _drawDevice.Draw(_drawDevice.WhitePixel, new Vector2(200f, 500f), RgbaFloat.White, Matrix3x2.CreateRotation(rotation));
            //drawDevice.Draw(drawDevice.Grid, new RectangleF(objectPosition.X - 50f, objectPosition.Y, 30f, 30f), RgbaFloat.Red);
            //drawDevice.Draw(drawDevice.Grid, new RectangleF(objectPosition.X + 50f, objectPosition.Y, 30f, 30f));
            //drawDevice.Draw(drawDevice.Grid, triangleVertices);
            //drawDevice.Draw(drawDevice.WhitePixel, triangleVertices, Matrix3x2.CreateRotation(1) * Matrix3x2.CreateTranslation(new Vector2(-50, -50)));
            //drawDevice.Draw(drawDevice.Grid, new Vector2(30f, 30f), Matrix3x2.CreateTranslation(50, 0));
            //drawDevice.Draw(drawDevice.Grid, new Vector2(30f, 30f), RgbaFloat.CornflowerBlue, Matrix3x2.CreateTranslation(100, 0));
            //drawDevice.Draw(drawDevice.WhitePixel, new Vector2(40f, 40f), new Vector2(-100, 0), -1);
            //drawDevice.Draw(drawDevice.WhitePixel, new Vector2(40f, 40f), new Vector2(-100, 100), -0.5f, RgbaFloat.Grey);
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