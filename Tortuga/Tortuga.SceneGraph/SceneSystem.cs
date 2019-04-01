using OpenSkies.Drawing;
using OpenSkies.SceneGraph;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSkies.SceneGraph
{
    public class SceneSystem
    {
        public Scene CurrentScene { get; internal set; }

        public virtual void Update(float time) { }
        public virtual void RenderSprites(DrawDevice batch, float time) { }
    }
}
