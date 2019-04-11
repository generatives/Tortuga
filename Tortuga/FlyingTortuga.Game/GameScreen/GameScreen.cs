using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Drawing;

namespace FlyingTortuga.Game.GameScreen
{
    class GameScreen : IScreen
    {
        private Game _game;
        private DrawDevice _drawDevice;

        private Player _player;
        private float _previousPosition;

        private float _obstacleSpacing = 800;
        private List<Obstacle> _obstacles;

        private Random _rand = new Random();

        public void Started(Game game)
        {
            _game = game;
            _drawDevice = new DrawDevice(_game.Window.GraphicsDevice, _game.Window.MainSwapchain);

            _player = new Player(_game.Window.InputTracker);
            _obstacles = new List<Obstacle>();
        }

        public void Tick(float deltaTime)
        {
            Update(deltaTime);
            Render();
        }

        private void Update(float deltaTime)
        {
            _previousPosition = _player.Position.X;
            _player.Update(deltaTime);
            if(_previousPosition % _obstacleSpacing > _player.Position.X % _obstacleSpacing)
            {
                SpawnObstacle(_player.Position.X + _obstacleSpacing);
            }
            foreach(var obstacle in _obstacles)
            {
                obstacle.Update(deltaTime);
            }
        }

        private void Render()
        {
            _drawDevice.Begin(Matrix4x4.CreateTranslation(-_player.Position.X, 0, 0) *
                Matrix4x4.CreateScale(1f / _game.Window.Width, 1f / _game.Window.Height, 1f));
            _player.Render(_drawDevice);
            foreach (var obstacle in _obstacles)
            {
                obstacle.Render(_drawDevice);
            }
            _drawDevice.End();
        }

        private void SpawnObstacle(float position)
        {
            var size = new Vector2(_rand.Next(30, 100), _rand.Next(100, 500));
            _obstacles.Add(new Obstacle(new Vector2(position, -100), size, _player, this));
        }

        public void Stopped()
        {
            _drawDevice.Dispose();
        }
    }
}
