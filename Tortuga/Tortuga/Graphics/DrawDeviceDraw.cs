using Tortuga.Geometry;
using Tortuga.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using System.Linq;
using Tortuga.Graphics.Resources;

namespace Tortuga.Graphics
{
    public static class DrawDeviceExt
    {
        public static void Add(this DrawDevice device, Surface surface, RectangleF rect, RgbaFloat? color = null)
        {
            device.Add(surface, RectangleF.ZeroRect(surface.Width, surface.Height), rect, color);
        }

        public static void Add(this DrawDevice device, Surface surface, Vector2 size, Matrix3x2 transform, RgbaFloat? color = null)
        {
            device.Add(surface, RectangleF.ZeroRect(surface.Width, surface.Height), size, transform, color);
        }

        public static void Add(this DrawDevice device, SubSurface surface, RectangleF rect, RgbaFloat? color = null)
        {
            device.Add(surface.Surface, surface.SourceRect, rect, color);
        }

        public static void Add(this DrawDevice device, SubSurface surface, Vector2 size, Matrix3x2 transform, RgbaFloat? color = null)
        {
            device.Add(surface.Surface, surface.SourceRect, size, transform, color);
        }
    }
}
