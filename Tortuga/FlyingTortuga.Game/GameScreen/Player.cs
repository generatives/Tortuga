using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics;
using Tortuga.Geometry;
using Tortuga.Platform;
using Veldrid;
using Tortuga.Graphics.Resources;

namespace FlyingTortuga.Game.GameScreen
{
    class Player
    {
        public Vector2 Position { get; private set; }

        private Vector2 _velocity = new Vector2(200, 0);
        private Vector2 _maxVelocity = new Vector2(100000, 500);
        private Vector2 _acceleration = new Vector2(1, -500);

        private float _jumpSpeed = 300;

        public Vector2 Size { get; private set; } = new Vector2(32, 32);

        private IInputTracker _inputTracker;
        private SpriteSheet _sheet;
        private float _timeSinceFlap;
        private float _wingsDownTime = 0.1f;

        public bool Go { get; set; }

        public Player(SpriteSheet sheet, IInputTracker input)
        {
            _sheet = sheet;
            _inputTracker = input;
            _timeSinceFlap = _wingsDownTime + 1;
        }

        public void Update(float deltaTime)
        {
            if (!Go) return;

            if(_inputTracker.GetKeyDown(TKey.Space))
            {
                _velocity = new Vector2(_velocity.X, _jumpSpeed);
                _timeSinceFlap = 0;
            }

            _timeSinceFlap += deltaTime;

            _velocity += _acceleration * deltaTime;
            if (_velocity.X > _maxVelocity.X) _velocity = new Vector2(_maxVelocity.X, _velocity.Y);
            if (_velocity.X < -_maxVelocity.X) _velocity = new Vector2(-_maxVelocity.X, _velocity.Y);
            if (_velocity.Y > _maxVelocity.Y) _velocity = new Vector2(_velocity.X, _maxVelocity.Y);
            if (_velocity.Y < -_maxVelocity.Y) _velocity = new Vector2(_velocity.X, -_maxVelocity.Y);

            Position += _velocity * deltaTime;
        }

        public void Render(DrawDevice drawDevice)
        {
            var frame = _timeSinceFlap < _wingsDownTime ? _sheet[1] : _sheet[0];
            drawDevice.Add(frame, GetBoundingBox());
        }

        public RectangleF GetBoundingBox()
        {
            return new RectangleF(Position.X, Position.Y, Size.X, Size.Y);
        }
    }
}
