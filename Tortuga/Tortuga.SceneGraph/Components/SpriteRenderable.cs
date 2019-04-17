﻿using Tortuga.Graphics;
using Tortuga.Graphics.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortuga.SceneGraph.Components
{
    public class SpriteRenderable : Component, IRenderable
    {
        public SubTexture Texture { get; set; }

        public void Render(DrawDevice device, float time)
        {

            device.Draw(Texture, Texture.Size, GameObject.Transform.Position, GameObject.Transform.Rotation);
        }
    }
}
