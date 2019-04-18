using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tortuga.Assets
{
    public interface IAssetSource
    {
        Stream LoadAsset(string key);
    }
}
