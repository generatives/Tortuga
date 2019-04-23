using System;
using System.Diagnostics;
using Tortuga.Assets;
using Tortuga.Graphics;
using Tortuga.Platform;

namespace FlyingTortuga.Game
{
    public class Game
    {
        public IScreen CurrentScreen { get; private set; }
        public IScreen NextScreen { get; set; }
        public IWindow Window { get; private set; }
        public AssetLoader Assets { get; private set; }

        private double previousElapsed;
        private Stopwatch sw;

        public Game(IPlatform platform, IWindow window)
        {
            Window = window;
            NextScreen = new GameScreen.GameScreen();
            Assets = AssetLoader.DefaultAssetLoader(platform);
            Assets.AddSource(new EmbeddedResourceAssetSource(typeof(Game).Assembly, "Assets/"));
        }

        public void LoadResources()
        {
            sw = Stopwatch.StartNew();
            double previousElapsed = sw.Elapsed.TotalSeconds;
        }

        public void Tick()
        {
            double newElapsed = sw.Elapsed.TotalSeconds;
            sw.Restart();
            float deltaSeconds = (float)(newElapsed - previousElapsed);

            if(NextScreen != null)
            {
                CurrentScreen?.Stopped();
                CurrentScreen = NextScreen;
                CurrentScreen.Started(this);
                NextScreen = null;
            }

            CurrentScreen?.Tick(deltaSeconds);
        }
    }
}
