using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSkies.SceneGraph.Components
{
    public class Component
    {
        public GameObject GameObject { get; internal set; }
        public Scene CurrentScene { get => GameObject?.CurrentScene; }
    }
}
