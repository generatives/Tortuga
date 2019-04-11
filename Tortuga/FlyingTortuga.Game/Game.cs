using System;
using System.Diagnostics;
using Tortuga.Drawing;
using Tortuga.Platform;

namespace FlyingTortuga.Game
{
    public class Game
    {
        private IScreen _currentScreen;
        private IScreen _nextScreen;
        public IWindow Window { get; private set; }

        private double previousElapsed;
        private Stopwatch sw;

        public Game(IWindow window)
        {
            Window = window;
            _nextScreen = new GameScreen.GameScreen();
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

            if(_nextScreen != null)
            {
                _currentScreen?.Stopped();
                _nextScreen.Started(this);
                _currentScreen = _nextScreen;
                _nextScreen = null;
            }

            _currentScreen?.Tick(deltaSeconds);
        }
    }
}
