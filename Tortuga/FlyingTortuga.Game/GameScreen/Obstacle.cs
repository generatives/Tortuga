using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Tortuga.Geometry;
using Veldrid;
using Tortuga.Graphics.Resources;

namespace FlyingTortuga.Game.GameScreen
{
    class Obstacle
    {
        private bool _hit;
        private RectangleF _rect;

        private GameScreen _gameScreen;
        private Surface _surface;

        public Obstacle(Vector2 position, Vector2 size, GameScreen gameScreen, Surface surface)
        {
            _rect = new RectangleF(position.X, position.Y, size.X, size.Y);
            _gameScreen = gameScreen;
            _surface = surface;
        }

        public void Update(float deltaTime)
        {
            var playerRect = _gameScreen.Player.GetBoundingBox();
            if(playerRect.Overlaps(_rect))
            {
                _hit = true;
                _gameScreen.PlayerHitObstacle();
            }
        }

        public void Render(DrawDevice drawDevice)
        {
            drawDevice.Add(_surface, RectangleF.ZeroRect(16, 32), _rect, RgbaFloat.White);
        }
    }
}
