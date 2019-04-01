using Tortuga.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Veldrid;

namespace Tortuga.Platform
{
    public interface IPlatform
    {
        ISound CreateSound(string resourceId);
    }
}
