using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenSkies.Drawing
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DrawInfo
    {
        public Matrix3x2 Transform;
        public Vector2 ScreenSize;

        public DrawInfo(Matrix3x2 transform, Vector2 screenSize)
        {
            Transform = transform;
            ScreenSize = screenSize;
        }
    }
}
