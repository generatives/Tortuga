using Cyotek.Drawing.BitmapFont;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tortuga.Audio;
using Tortuga.Graphics.Resources;
using Tortuga.Platform;
using Veldrid;

namespace Tortuga.Assets
{
    public class AssetLoader
    {
        private IPlatform _platform;
        private List<IAssetSource> _sources;

        private Dictionary<string, object> _assetCache;

        public static EmbeddedResourceAssetSource CoreAssetSource { get; private set; }
            = new EmbeddedResourceAssetSource(typeof(AssetLoader).Assembly, "CoreAssets/");

        public static AssetLoader DefaultAssetLoader(IPlatform platform)
        {
            var loader = new AssetLoader(platform);
            loader.AddSource(CoreAssetSource);
            loader.AddSources(platform.GetDefaultAssetSources());
            return loader;
        }

        public AssetLoader(IPlatform platform)
        {
            _platform = platform;
            _sources = new List<IAssetSource>();
            _assetCache = new Dictionary<string, object>();
        }

        public void AddSources(IEnumerable<IAssetSource> sources)
        {
            _sources.AddRange(sources);
        }

        public void RemoveSources(IEnumerable<IAssetSource> sources)
        {
            foreach(var source in sources)
            {
                _sources.Remove(source);
            }
        }

        public void AddSource(IAssetSource source)
        {
            _sources.Add(source);
        }

        private void RemoveSource(IAssetSource source)
        {
            _sources.Remove(source);
        }

        public Image<Rgba32> LoadImage(string key)
        {
            var cachedAsset = GetCachedAsset<Image<Rgba32>>(key);
            if (cachedAsset != null) return cachedAsset;

            var assetStream = LoadAssetStream(key);
            if (assetStream == null) return null;

            var image = Image.Load(assetStream);
            _assetCache[key] = image;
            return image;
        }

        public ISound LoadSound(string key)
        {
            var cachedAsset = GetCachedAsset<ISound>(key);
            if (cachedAsset != null) return cachedAsset;

            var assetStream = LoadAssetStream(key);
            if (assetStream == null) return null;

            var sound = _platform.CreateSound(assetStream);
            _assetCache[key] = sound;
            return sound;
        }

        public BitmapFont LoadFont(string key)
        {
            var cachedAsset = GetCachedAsset<BitmapFont>(key);
            if (cachedAsset != null) return cachedAsset;

            var assetStream = LoadAssetStream(key);
            if (assetStream == null) return null;

            var font = new BitmapFont();
            font.Load(assetStream);
            _assetCache[key] = font;
            return font;
        }

        private T GetCachedAsset<T>(string key)
        {
            if (_assetCache.ContainsKey(key))
            {
                var asset = _assetCache[key];
                if (asset is T typedAsset)
                {
                    return typedAsset;
                }
                else
                {
                    throw new Exception($"Asset with key: {key} is not of type: {nameof(T)}");
                }
            }
            else
            {
                return default(T);
            }
        }

        private Stream LoadAssetStream(string key)
        {
            foreach(var source in _sources)
            {
                var asset = source.LoadAsset(key);
                if(asset != null)
                {
                    return asset;
                }
            }
            return null;
        }
    }
}
