using Tortuga.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Veldrid;
using Tortuga.Assets;

namespace Tortuga.Platform
{
    public interface IPlatform
    {
        ISound CreateSound(Stream stream);
        IEnumerable<IAssetSource> GetDefaultAssetSources();
    }
}
