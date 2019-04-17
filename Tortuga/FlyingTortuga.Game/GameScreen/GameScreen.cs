using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;

namespace FlyingTortuga.Game.GameScreen
{
    class GameScreen : IScreen
    {
        private Game _game;
        private DrawDevice _drawDevice;
        private ViewportManager _viewportManager;

        private Player _player;

        private int MIN_OBSTACLE_SPACING = 400;
        private int MAX_OBSTACLE_SPACING = 900;
        private int _nextSpawnPosition = 500;
        private List<Obstacle> _obstacles;

        private Random _rand = new Random();

        private int SCREEN_WIDTH = 1280;
        private int SCREEN_HEIGHT = 720;

        private int MIN_OBSTACLE_WIDTH = 20;
        private int MAX_OBSTACLE_WIDTH = 100;
        private int MIN_OBSTACLE_HEIGHT = 10;
        private int MIN_GAP_SIZE = 100;
        private int MAX_GAP_SIZE = 400;

        public void Started(Game game)
        {
            _game = game;
            _game.Window.Resized += Window_Resized;
            _drawDevice = new DrawDevice(_game.Window.GraphicsDevice, _game.Window.MainSwapchain);
            _viewportManager = new ViewportManager(SCREEN_WIDTH, SCREEN_HEIGHT);
            _viewportManager.WindowChanged(_game.Window.Width, _game.Window.Height);

            _player = new Player(_game.Window.InputTracker);
            _obstacles = new List<Obstacle>();
        }

        private void Window_Resized()
        {
            _viewportManager.WindowChanged(_game.Window.Width, _game.Window.Height);
        }

        public void Tick(float deltaTime)
        {
            Update(deltaTime);
            Render();
        }

        private void Update(float deltaTime)
        {
            _player.Update(deltaTime);
            if(_player.Position.X + SCREEN_WIDTH / 2 >= _nextSpawnPosition)
            {
                SpawnObstacle(_nextSpawnPosition);
                _nextSpawnPosition = _nextSpawnPosition + _rand.Next(MIN_OBSTACLE_SPACING, MAX_OBSTACLE_SPACING);
            }
            foreach(var obstacle in _obstacles)
            {
                obstacle.Update(deltaTime);
            }
        }

        private void Render()
        {
            _drawDevice.Begin(
                Matrix4x4.CreateTranslation(-_player.Position.X, 0, 0) * _viewportManager.GetScalingTransform(),
                _viewportManager.Viewport);
            _player.Render(_drawDevice);
            foreach (var obstacle in _obstacles)
            {
                obstacle.Render(_drawDevice);
            }
            _drawDevice.End();
        }

        private void SpawnObstacle(float position)
        {
            var gapSize = _rand.Next(MIN_GAP_SIZE, MAX_GAP_SIZE);
            var location = _rand.Next(MIN_OBSTACLE_HEIGHT, SCREEN_HEIGHT - MIN_OBSTACLE_HEIGHT - gapSize);
            var width = _rand.Next(MIN_OBSTACLE_WIDTH, MAX_OBSTACLE_WIDTH);

            var bottomSize = new Vector2(width, location);
            _obstacles.Add(new Obstacle(new Vector2(position, -SCREEN_HEIGHT / 2), bottomSize, _player, this));

            var topSize = new Vector2(width, SCREEN_HEIGHT - location - gapSize);
            _obstacles.Add(new Obstacle(new Vector2(position, location + gapSize - (SCREEN_HEIGHT / 2)), topSize, _player, this));
        }

        public void Stopped()
        {
            _drawDevice.Dispose();
        }
    }
}
