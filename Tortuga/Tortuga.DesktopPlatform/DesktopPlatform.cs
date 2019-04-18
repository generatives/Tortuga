using Tortuga.Audio;
using Tortuga.Platform;
using SharpAudio;
using System;
using System.IO;
using Veldrid;
using Veldrid.StartupUtilities;
using System.Collections.Generic;
using Tortuga.Assets;

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

        public ISound CreateSound(Stream stream)
        {
            return new DesktopSound(stream, _engine);
        }

        public IEnumerable<IAssetSource> GetDefaultAssetSources()
        {
            return new[] { new FileSystemAssetSource("Assets/") };
        }
    }
}
