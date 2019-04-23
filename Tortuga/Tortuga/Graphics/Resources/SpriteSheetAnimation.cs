using System;
using System.Collections.Generic;
using System.Text;

namespace Tortuga.Graphics.Resources
{
    public class SpriteSheetAnimation
    {
        private SpriteSheet _gridSpriteSheet;
        public float FPS
        {
            get
            {
                return 1f / FramePeriod;
            }
            set
            {
                FramePeriod = 1f / value;
            }
        }

        public float FramePeriod { get; set; }

        public SubSurface CurrentFrame
        {
            get
            {
                return _gridSpriteSheet[_frames[_currentFrame]];
            }
        }

        private int _currentFrame;
        private float _timeInCurrentFrame;
        private int[] _frames;

        public SpriteSheetAnimation(SpriteSheet spriteSheet, int[] frames, float fps)
        {
            _gridSpriteSheet = spriteSheet;
            FPS = fps;
            _frames = frames;
        }

        public void Update(float delta)
        {
            _timeInCurrentFrame += delta;
            if(_timeInCurrentFrame >= FramePeriod)
            {
                _currentFrame++;
                if(_currentFrame > _frames.Length)
                {
                    _currentFrame = 0;
                }
                _timeInCurrentFrame -= FramePeriod;
            }
        }

        public void Reset()
        {
            _currentFrame = 0;
            _timeInCurrentFrame = 0;
        }
    }
}
