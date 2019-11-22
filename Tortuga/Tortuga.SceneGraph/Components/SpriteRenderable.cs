using Tortuga.Graphics;
using Tortuga.Graphics.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Tortuga.SceneGraph.Components
{
    public class SpriteRenderable : Component, IRenderable
    {
        public SubSurface Texture { get; set; }

        public void Render(DrawDevice device, float time)
        {
            var transform = Matrix3x2.CreateScale(GameObject.Transform.Scale) *
                Matrix3x2.CreateRotation(GameObject.Transform.Rotation) *
                Matrix3x2.CreateTranslation(GameObject.Transform.Position);
            device.Add(Texture, Texture.SourceRect.Size, transform);
        }
    }
}
