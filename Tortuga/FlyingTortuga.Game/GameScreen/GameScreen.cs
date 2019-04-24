using Cyotek.Drawing.BitmapFont;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Geometry;
using Tortuga.Graphics;
using Tortuga.Graphics.Resources;
using Tortuga.Graphics.Text;

namespace FlyingTortuga.Game.GameScreen
{
    class GameScreen : IScreen
    {
        private Game _game;
        private DrawDevice _drawDevice;
        private ViewportManager _viewportManager;

        private Surface _background;
        private TextRenderer _textRenderer;

        public Player Player { get; private set; }

        private int MIN_OBSTACLE_SPACING = 150;
        private int MAX_OBSTACLE_SPACING = 300;
        private int _nextSpawnPosition = 500;
        private List<Obstacle> _obstacles;
        private List<ObstaclePrototype> _prototypes;

        private Random _rand = new Random();

        private GameState _state = GameState.WAITING;

        private float PLAYER_DEATH_WAIT_TIME = 0.25f;
        private float _remainingWaitTime;

        public static int SCREEN_WIDTH = 1280;
        public static int SCREEN_HEIGHT = 720;

        public void Started(Game game)
        {
            _game = game;
            _game.Window.Resized += Window_Resized;
            _drawDevice = new DrawDevice(_game.Window.GraphicsDevice, _game.Window.MainSwapchain);
            _viewportManager = new ViewportManager(SCREEN_WIDTH, SCREEN_HEIGHT);
            _viewportManager.WindowChanged(_game.Window.Width, _game.Window.Height);

            _background = _drawDevice.CreateSurface(game.Assets.LoadImage("Background.png"));
            _textRenderer = new TextRenderer(game.Assets.LoadFont(BitmapFont.DefaultFontName), game.Assets, _drawDevice);

            var playerImage = game.Assets.LoadImage("FlyingTortuga.png");
            var playerSurface = _drawDevice.CreateSurface(playerImage);
            var spriteSheet = SpriteSheet.CreateGrid(playerSurface, 16, 16, 2, false);

            Player = new Player(spriteSheet, _game.Window.InputTracker);
            _obstacles = new List<Obstacle>();

            FillObstaclePrototypes();
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
            _drawDevice.Begin(_viewportManager.GetScalingTransform(), _viewportManager.Viewport);
            _drawDevice.Add(_background, new RectangleF(-(SCREEN_WIDTH / 2f), -(SCREEN_HEIGHT / 2f), SCREEN_WIDTH, SCREEN_HEIGHT));
            _textRenderer.DrawText($"Distance: {(int)Player.Position.X / 16}", new Vector2(-(SCREEN_WIDTH / 2) + 10, (SCREEN_HEIGHT / 2) - 40));

            _drawDevice.Transform = Matrix4x4.CreateTranslation(-Player.Position.X - SCREEN_WIDTH * 0.4f, 0, 0) *
                _viewportManager.GetScalingTransform();
            Player.Render(_drawDevice);
            foreach (var obstacle in _obstacles)
            {
                obstacle.Render(_drawDevice);
            }
            _drawDevice.End();

            _game.Window.GraphicsDevice.SwapBuffers(_game.Window.MainSwapchain);
            _game.Window.GraphicsDevice.WaitForIdle();
        }

        private void SpawnObstacle(float position)
        {
            var prototype = _prototypes[_rand.Next(0, _prototypes.Count)];

            var location = _rand.Next(prototype.MinHeight, prototype.MaxHeight);
            var scale = (float)_rand.NextDouble() * (prototype.MaxScale - prototype.MinScale) + prototype.MinScale;

            var size = new Vector2(prototype.Width * scale, prototype.Height * scale);
            _obstacles.Add(new Obstacle(new Vector2(position, -SCREEN_HEIGHT / 2 + location), size, this, prototype.Surface));
        }

        public void PlayerHitObstacle()
        {
            _state = GameState.DIED;
            Player.Go = false;
            _remainingWaitTime = PLAYER_DEATH_WAIT_TIME;
        }

        public void AddPrototype(ObstaclePrototype prototype, int num)
        {
            for(int i = 0; i < num; i++)
            {
                _prototypes.Add(prototype);
            }
        }

        public void FillObstaclePrototypes()
        {
            _prototypes = new List<ObstaclePrototype>();
            AddPrototype(
                new ObstaclePrototype()
                {
                    MinHeight = 0,
                    MaxHeight = 0,
                    MinScale = SCREEN_HEIGHT * 0.1f / 64f,
                    MaxScale = SCREEN_HEIGHT * 0.4f / 64f,
                    Surface = _drawDevice.CreateSurface(_game.Assets.LoadImage("PalmTree.png"))
                }, 1);
            AddPrototype(
                new ObstaclePrototype()
                {
                    MinHeight = SCREEN_HEIGHT / 3,
                    MaxHeight = SCREEN_HEIGHT,
                    MinScale = 5f,
                    MaxScale = 8f,
                    Surface = _drawDevice.CreateSurface(_game.Assets.LoadImage("Bird.png"))
                }, 3);
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
