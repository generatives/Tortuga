using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSkies.SceneGraph.Components
{
    interface ISceneEventListener
    {
        void CurrentSceneStarted();
        void CurrentSceneStopped();
    }
}
