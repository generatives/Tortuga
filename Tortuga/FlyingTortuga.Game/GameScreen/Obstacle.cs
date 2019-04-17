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
        public Vector2 Position { get; private set; }
        private Vector2 _size = new Vector2(50, 30);

        private Player _player;
        private GameScreen _gameScreen;

        public Obstacle(Vector2 position, Vector2 size, Player player, GameScreen gameScreen)
        {
            Position = position;
            _size = size;
            _player = player;
            _gameScreen = gameScreen;
        }

        public void Update(float deltaTime)
        {

        }

        public void Render(DrawDevice drawDevice)
        {
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1), GetCurrentRectangle(), RgbaFloat.White);
        }

        public RectangleF GetCurrentRectangle()
        {
            return new RectangleF(Position.X, Position.Y, _size.X, _size.Y);
        }
    }
}
