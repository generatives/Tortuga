﻿using Tortuga.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Tortuga.DesktopPlatform
{
    public class DesktopWindow : IWindow
    {
        public Sdl2Window Window { get; private set; }

        public event Action Resized;
        public event Action Tick;
        public event Action GraphicsDeviceCreated;

        public int Width => Window.Width;

        public int Height => Window.Height;

        private bool _windowResized;
        private bool _paused;

        public GraphicsDevice GraphicsDevice { get; private set; }
        public Swapchain MainSwapchain => GraphicsDevice.MainSwapchain;
        private DesktopInputTracker _inputTracker;
        public IInputTracker InputTracker { get => _inputTracker; }

        private WindowCreateInfo _windowCreateInfo;
        private GraphicsDeviceOptions _graphicsDeviceOptions;

        public DesktopWindow(WindowCreateInfo arg, GraphicsDeviceOptions options)
        {
            _windowCreateInfo = arg;
            _graphicsDeviceOptions = options;
            _inputTracker = new DesktopInputTracker();
        }

        public Task Run()
        {
            return Task.Factory.StartNew(() =>
            {
                Window = VeldridStartup.CreateWindow(ref _windowCreateInfo);
                Window.Resized += () => _windowResized = true;
                GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, _graphicsDeviceOptions, GraphicsBackend.Vulkan);

                GraphicsDeviceCreated?.Invoke();

                Resized?.Invoke();

                while (Window.Exists)
                {
                    if (_paused) continue;

                    var inputSnapshot = Window.PumpEvents();

                    if (!Window.Exists) break;

                    if (_windowResized)
                    {
                        _windowResized = false;
                        GraphicsDevice.ResizeMainWindow((uint)Window.Width, (uint)Window.Height);
                        Resized?.Invoke();
                    }
                    _inputTracker.UpdateFrameInput(inputSnapshot);
                    Tick?.Invoke();
                }
            }, TaskCreationOptions.LongRunning);
        }

        public void Pause()
        {
            _paused = true;
        }

        public void Resume()
        {
            _paused = false;
        }
    }
}
