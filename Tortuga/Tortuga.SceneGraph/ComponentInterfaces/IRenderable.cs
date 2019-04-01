using OpenSkies.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSkies.SceneGraph.Components
{
    public interface IRenderable
    {
        void Render(DrawDevice device, float time);
    }
}
