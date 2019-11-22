using Cyotek.Drawing.BitmapFont;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tortuga.Assets.Importers;
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
        private Dictionary<Type, IImporter> _importers;

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

        public AssetLoader(IPlatform platform, Dictionary<Type, IImporter> importers = null)
        {
            _platform = platform;
            _sources = new List<IAssetSource>();
            _assetCache = new Dictionary<string, object>();

            _importers = importers ?? new Dictionary<Type, IImporter>();

            var requiredImporters = new Dictionary<Type, IImporter>()
            {
                { typeof(Image<Rgba32>), new ImageImporter() },
                { typeof(ISound), new SoundImporter(platform) },
                { typeof(BitmapFont), new FontImporter() },
            };

            foreach (var importer in requiredImporters)
            {
                if (!_importers.ContainsKey(importer.Key))
                {
                    _importers[importer.Key] = importer.Value;
                }
            }
        }

        public void AddImporter<T>(IImporter importer)
        {
            _importers[typeof(T)] = importer;
        }

        public void AddImporter(Type type, IImporter importer)
        {
            _importers[type] = importer;
        }

        public void RemoveImporter<T>()
        {
            _importers.Remove(typeof(T));
        }

        public void RemoveImporter(Type type)
        {
            _importers.Remove(type);
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

        public void RemoveSource(IAssetSource source)
        {
            _sources.Remove(source);
        }

        public T Load<T>(string key)
        {
            var cachedAsset = GetCachedAsset<T>(key);
            if (cachedAsset != null) return cachedAsset;

            var assetStream = LoadAssetStream(key);
            if (assetStream == null) return default(T);

            if(!_importers.ContainsKey(typeof(T))) return default(T);

            var asset = _importers[typeof(T)].Import(assetStream);
            _assetCache[key] = asset;
            return (T)asset;
        }

        public Image<Rgba32> LoadImage(string key)
        {
            return Load<Image<Rgba32>>(key);
        }

        public ISound LoadSound(string key)
        {
            return Load<ISound>(key);
        }

        public BitmapFont LoadFont(string key)
        {
            return Load<BitmapFont>(key);
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
