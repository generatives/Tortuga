using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Tortuga.Assets
{
    public class EmbeddedResourceAssetSource : IAssetSource
    {
        public Assembly Assembly { get; private set; }
        public string BaseDir { get; private set; }

        public EmbeddedResourceAssetSource(Assembly assembly, string baseDir = "")
        {
            Assembly = assembly;
            BaseDir = baseDir;
        }

        public Stream LoadAsset(string key)
        {
            return Assembly.GetManifestResourceStream(GetResourceName(key));
        }

        private string GetResourceName(string key)
        {
            var path = BaseDir + key;
            return Assembly.GetName().Name + "." + path.Replace(" ", "_")
                                                               .Replace("\\", ".")
                                                               .Replace("/", ".");
        }
    }
}
