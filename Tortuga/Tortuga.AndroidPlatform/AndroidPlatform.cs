using Android.App;
using Android.Media;
using Tortuga.Audio;
using Tortuga.Platform;
using System;
using System.IO;
using Veldrid;
using System.Collections.Generic;
using Tortuga.Assets;

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

        public ISound CreateSound(System.IO.Stream stream)
        {
            var player = new MediaPlayer();
            player.SetDataSource(new StreamMediaDataSource(stream));
            player.Prepare();
            return new AndroidSound(player);
        }

        public IEnumerable<IAssetSource> GetDefaultAssetSources()
        {
            return new[] { new AndroidResourceAssetSource(_activity.Assets) };
        }
    }

    public class StreamMediaDataSource : MediaDataSource
    {
        System.IO.Stream data;

        public StreamMediaDataSource(System.IO.Stream Data)
        {
            data = Data;
        }

        public override long Size
        {
            get
            {
                return data.Length;
            }
        }

        public override int ReadAt(long position, byte[] buffer, int offset, int size)
        {
            data.Seek(position, System.IO.SeekOrigin.Begin);
            return data.Read(buffer, offset, size);
        }

        public override void Close()
        {
            if (data != null)
            {
                data.Dispose();
                data = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (data != null)
            {
                data.Dispose();
                data = null;
            }
        }
    }
}
