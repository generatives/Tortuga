using Android.App;
using Android.Media;
using Tortuga.Audio;
using Tortuga.Platform;
using System;
using System.IO;
using Veldrid;

namespace Tortuga.AndroidPlatform
{
    public class AndroidPlatform : IPlatform
    {
        private Activity _activity;
        private VeldridSurfaceView _veldridSurfaceView;
        private AndroidWindow _window;

        public AndroidPlatform(Activity activity)
        {
            _activity = activity;
        }

        public IWindow CreateWindow(GraphicsDeviceOptions options)
        {
            if(_window != null)
            {
                return null;
            }
            else
            {
                GraphicsBackend backend = GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)
                    ? GraphicsBackend.Vulkan
                    : GraphicsBackend.OpenGLES;
                _veldridSurfaceView = new VeldridSurfaceView(_activity, backend, options);
                _activity.SetContentView(_veldridSurfaceView);
                _window = new AndroidWindow(_veldridSurfaceView);
                return _window;
            }
        }

        public ISound CreateSound(string resourceId)
        {
            return new AndroidSound(MediaPlayer.Create(_activity, GetUriFromResourceId(resourceId)));
        }

        private Android.Net.Uri GetUriFromResourceId(string resourceId)
        {
            return Android.Net.Uri.Parse("android.resource://" + _activity.PackageName + "/" + resourceId);
        }
    }
}
