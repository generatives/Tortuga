using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tortuga.Graphics.Resources;
using Veldrid;

namespace Tortuga.Graphics
{
    public struct Batch
    {
        public Surface Surface;
        public uint NumVertices;
        public Matrix4x4 Transform;
    }
}
