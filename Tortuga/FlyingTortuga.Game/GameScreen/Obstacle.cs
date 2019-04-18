using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Tortuga.Geometry;
using Veldrid;

namespace FlyingTortuga.Game.GameScreen
{
    class Obstacle
    {
        private bool _hit;
        private RectangleF _rect;

        private Player _player;
        private GameScreen _gameScreen;

        public Obstacle(Vector2 position, Vector2 size, Player player, GameScreen gameScreen)
        {
            _rect = new RectangleF(position.X, position.Y, size.X, size.Y);
            _player = player;
            _gameScreen = gameScreen;
        }

        public void Update(float deltaTime)
        {
            var playerRect = new RectangleF(_player.Position, _player.Size);
            if(playerRect.Overlaps(_rect))
            {
                _hit = true;
                _gameScreen.PlayerHitObstacle();
            }
        }

        public void Render(DrawDevice drawDevice)
        {
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1), _rect, _hit ? RgbaFloat.Red : RgbaFloat.White);
        }
    }
}
