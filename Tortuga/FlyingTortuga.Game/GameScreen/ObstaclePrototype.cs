using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Graphics.Resources;

namespace FlyingTortuga.Game.GameScreen
{
    public class ObstaclePrototype
    {
        public int MinHeight { get; set; }
        public int MaxHeight { get; set; }
        public float MinScale { get; set; }
        public float MaxScale { get; set; }
        public Surface Surface { get; set; }
        public uint Height => Surface.Height;
        public uint Width => Surface.Width;
    }
}
