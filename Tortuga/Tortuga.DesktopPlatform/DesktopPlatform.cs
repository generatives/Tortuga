using Tortuga.Audio;
using Tortuga.Platform;
using SharpAudio;
using System;
using System.IO;
using Veldrid;
using Veldrid.StartupUtilities;

namespace Tortuga.DesktopPlatform
{
    public class DesktopPlatform : IPlatform
    {
        private AudioEngine _engine;

        public DesktopPlatform()
        {
            _engine = AudioEngine.CreateDefault();
        }

        public IWindow CreateWindow(WindowCreateInfo arg, GraphicsDeviceOptions options)
        {
            return new DesktopWindow(arg, options);
        }

        public ISound CreateSound(string resourceId)
        {
            return new DesktopSound(File.OpenRead(resourceId), _engine);
        }
    }
}
