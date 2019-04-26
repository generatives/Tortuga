using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Tortuga.Assets;

namespace Tortuga.AndroidPlatform
{
    public class AndroidResourceAssetSource : IAssetSource
    {
        public AssetManager Assets { get; private set; }

        public AndroidResourceAssetSource(AssetManager assets)
        {
            Assets = assets;
        }

        public Stream LoadAsset(string key)
        {
            try
            {
                return Assets.Open(key);
            }
            catch(Java.IO.FileNotFoundException)
            {
                return null;
            }
        }
    }
}