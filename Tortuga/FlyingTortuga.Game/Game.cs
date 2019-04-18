using System;
using System.Diagnostics;
using Tortuga.Graphics;
using Tortuga.Platform;

namespace FlyingTortuga.Game
{
    public class Game
    {
        public IScreen CurrentScreen { get; private set; }
        public IScreen NextScreen { get; set; }
        public IWindow Window { get; private set; }

        private double previousElapsed;
        private Stopwatch sw;

        public Game(IWindow window)
        {
            Window = window;
            NextScreen = new GameScreen.GameScreen();
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
                GC.Collect();
                CurrentScreen.Started(this);
                NextScreen = null;
            }

            CurrentScreen?.Tick(deltaSeconds);
        }
    }
}
