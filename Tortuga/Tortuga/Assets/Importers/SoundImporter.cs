using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tortuga.Platform;

namespace Tortuga.Assets.Importers
{
    public class SoundImporter : IImporter
    {
        private IPlatform _platform;

        public SoundImporter(IPlatform platform)
        {
            _platform = platform;
        }

        public object Import(Stream stream)
        {
            return _platform.CreateSound(stream);
        }
    }
}
