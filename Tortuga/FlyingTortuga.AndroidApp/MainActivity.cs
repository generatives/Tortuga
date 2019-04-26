using Android.App;
using Android.Content.PM;
using Android.OS;
using Tortuga.Graphics;
using Tortuga.AndroidPlatform;
using Tortuga.Audio;
using Tortuga.Platform;
using System.Numerics;
using System.Threading;
using Veldrid;
using System;
using Tortuga.Geometry;
using FlyingTortuga.Game;

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

            var game = new Game(_platform, _window);
            _window.GraphicsDeviceCreated += game.LoadResources;
            _window.Tick += game.Tick;

            _window.Run();
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