using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Tortuga.Platform
{
    public interface IWindow
    {
        event Action Resized;
        event Action Tick;
        event Action GraphicsDeviceCreated;

        int Width { get; }
        int Height { get; }

        GraphicsDevice GraphicsDevice { get; }
        Swapchain MainSwapchain { get; }

        Task Run();
        void Pause();
        void Resume();
    }
}
