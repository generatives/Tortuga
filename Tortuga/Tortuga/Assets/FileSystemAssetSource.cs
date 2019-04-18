using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tortuga.Assets
{
    public class FileSystemAssetSource : IAssetSource
    {
        public string BaseDir { get; private set; }

        public FileSystemAssetSource(string baseDir = "")
        {
            BaseDir = baseDir;
        }

        public Stream LoadAsset(string key)
        {
            var path = BaseDir + key;
            if(File.Exists(path))
            {
                return File.OpenRead(path);
            }
            else
            {
                return null;
            }
        }
    }
}
