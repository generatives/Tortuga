using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Tortuga.Graphics.Resources;

namespace FlyingTortuga.Game.GameScreen
{
    class GameScreen : IScreen
    {
        private Game _game;
        private DrawDevice _drawDevice;
        private ViewportManager _viewportManager;

        public Player Player { get; private set; }

        private int MIN_OBSTACLE_SPACING = 400;
        private int MAX_OBSTACLE_SPACING = 900;
        private int _nextSpawnPosition = 500;
        private List<Obstacle> _obstacles;
        private Surface _obstacleSurface;

        private Random _rand = new Random();

        private GameState _state = GameState.WAITING;

        private float PLAYER_DEATH_WAIT_TIME = 0.25f;
        private float _remainingWaitTime;

        private int SCREEN_WIDTH = 1280;
        private int SCREEN_HEIGHT = 720;

        private int MIN_OBSTACLE_WIDTH = 20;
        private int MAX_OBSTACLE_WIDTH = 100;
        private int MIN_OBSTACLE_HEIGHT = 10;
        private int MIN_GAP_SIZE = 100;
        private int MAX_GAP_SIZE = 200;

        public void Started(Game game)
        {
            _game = game;
            _game.Window.Resized += Window_Resized;
            _drawDevice = new DrawDevice(_game.Window.GraphicsDevice, _game.Window.MainSwapchain);
            _viewportManager = new ViewportManager(SCREEN_WIDTH, SCREEN_HEIGHT);
            _viewportManager.WindowChanged(_game.Window.Width, _game.Window.Height);

            var obstacleImage = game.Assets.LoadImage("Coral.png");
            _obstacleSurface = _drawDevice.CreateSurface(obstacleImage);

            var playerImage = game.Assets.LoadImage("FlyingTortuga.png");
            var playerSurface = _drawDevice.CreateSurface(playerImage);
            var spriteSheet = SpriteSheet.CreateGrid(playerSurface, 16, 16, 2, false);

            Player = new Player(spriteSheet, _game.Window.InputTracker);
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
            if(_state == GameState.WAITING)
            {
                _state = _game.Window.InputTracker.GetKeyDown(Tortuga.Platform.TKey.Space) ? GameState.STARTED : GameState.WAITING;
                if(_state == GameState.STARTED)
                {
                    Player.Go = true;
                }
            }

            if(_state == GameState.DIED)
            {
                _remainingWaitTime -= deltaTime;
                if(_remainingWaitTime <= 0)
                {
                    _game.NextScreen = new GameScreen();
                }
                return;
            }

            Player.Update(deltaTime);
            if(Player.Position.X + SCREEN_WIDTH >= _nextSpawnPosition)
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
                Matrix4x4.CreateTranslation(-Player.Position.X - SCREEN_WIDTH * 0.4f, 0, 0) * _viewportManager.GetScalingTransform(),
                _viewportManager.Viewport);
            Player.Render(_drawDevice);
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
            _obstacles.Add(new Obstacle(new Vector2(position, -SCREEN_HEIGHT / 2), bottomSize, this, _obstacleSurface));

            var topSize = new Vector2(width, SCREEN_HEIGHT - location - gapSize);
            _obstacles.Add(new Obstacle(new Vector2(position, location + gapSize - (SCREEN_HEIGHT / 2)), topSize, this, _obstacleSurface));
        }

        public void PlayerHitObstacle()
        {
            _state = GameState.DIED;
            Player.Go = false;
            _remainingWaitTime = PLAYER_DEATH_WAIT_TIME;
        }

        public void Stopped()
        {
            _drawDevice.Dispose();
        }

        enum GameState
        {
            WAITING, STARTED, DIED
        }
    }
}
