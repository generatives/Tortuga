using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ultraviolet;

namespace Tortuga.SceneGraph.Components
{
    interface IUpdateable
    {
        void Update(float time);
    }
}
