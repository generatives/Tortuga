using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Drawing;
using Tortuga.Geometry;
using Tortuga.Platform;
using Veldrid;

namespace FlyingTortuga.Game.GameScreen
{
    class Player
    {
        public Vector2 Position { get; private set; }

        private Vector2 _velocity = new Vector2(200, 0);
        private Vector2 _maxVelocity = new Vector2(100000, 500);
        private Vector2 _acceleration = new Vector2(0, -500);

        private float _jumpSpeed = 400;

        private Vector2 _size = new Vector2(50, 30);

        private IInputTracker _inputTracker;

        public Player(IInputTracker input)
        {
            _inputTracker = input;
        }

        public void Update(float deltaTime)
        {
            if(_inputTracker.GetKeyDown(TKey.Space))
            {
                _velocity = new Vector2(_velocity.X, _jumpSpeed);
            }

            _velocity += _acceleration * deltaTime;
            if (_velocity.X > _maxVelocity.X) _velocity = new Vector2(_maxVelocity.X, _velocity.Y);
            if (_velocity.X < -_maxVelocity.X) _velocity = new Vector2(-_maxVelocity.X, _velocity.Y);
            if (_velocity.Y > _maxVelocity.Y) _velocity = new Vector2(_velocity.X, _maxVelocity.Y);
            if (_velocity.Y < -_maxVelocity.Y) _velocity = new Vector2(_velocity.X, -_maxVelocity.Y);

            Position += _velocity * deltaTime;
        }

        public void Render(DrawDevice drawDevice)
        {
            drawDevice.Add(drawDevice.WhitePixel, RectangleF.Square(1), GetCurrentRectangle(), RgbaFloat.Green);
        }

        public RectangleF GetCurrentRectangle()
        {
            return new RectangleF(Position.X, Position.Y, _size.X, _size.Y);
        }
    }
}
