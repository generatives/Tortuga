using Tortuga.AndroidPlatform;
using Tortuga.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Tortuga.AndroidPlatform
{
    public class AndroidWindow : IWindow
    {
        public event Action GraphicsDeviceCreated;
        public event Action Resized;
        public event Action Tick;

        public int Width => _veldridSurfaceView.Width;

        public int Height => _veldridSurfaceView.Height;

        public bool Exists => true;

        public GraphicsDevice GraphicsDevice => _veldridSurfaceView.GraphicsDevice;
        public Swapchain MainSwapchain => _veldridSurfaceView.MainSwapchain;

        private VeldridSurfaceView _veldridSurfaceView;

        public AndroidWindow(VeldridSurfaceView veldridSurfaceView)
        {
            _veldridSurfaceView = veldridSurfaceView;
            _veldridSurfaceView.DeviceCreated += () => GraphicsDeviceCreated?.Invoke();
            _veldridSurfaceView.Resized += () => Resized?.Invoke();
            _veldridSurfaceView.Tick += () => Tick?.Invoke();
        }

        public Task Run()
        {
            return _veldridSurfaceView.RunContinuousRenderLoop();
        }

        public void Pause()
        {
            _veldridSurfaceView.OnPause();
        }

        public void Resume()
        {
            _veldridSurfaceView.OnResume();
        }
    }
}
