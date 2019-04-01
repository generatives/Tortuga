using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenSkies.SceneGraph.Components
{
    public class Camera : Component
    {
        public Vector2 Zoom { get; set; }

        public Camera()
        {
            Zoom = Vector2.One;
        }

        public Matrix4x4 GetViewMatrix()
        {
            var position = GameObject.Transform.Position;
            var rotation = GameObject.Transform.Rotation;
            var viewport = Ultraviolet.UltravioletContext.DemandCurrent().GetGraphics().GetViewport();
            var origin = new Vector2(viewport.Width / 2f, viewport.Height / 2f);
            var matrix = Matrix4x4.CreateTranslation(-position.X, -position.Y, 0) *
                Matrix4x4.CreateTranslation(-origin.X, -origin.Y, 0f) *
                Matrix4x4.CreateRotationZ(-rotation * (2f * (float)Math.PI / 360f)) *
                Matrix4x4.CreateScale(Zoom.X, Zoom.Y, 1) *
                Matrix4x4.CreateTranslation(origin.X, origin.Y, 0f);
            return matrix;
        }
    }
}
