using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Tortuga.SceneGraph.Components
{
    public class Transform : Component
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }

        public Transform()
        {
            Scale = Vector2.One;
        }
    }
}
