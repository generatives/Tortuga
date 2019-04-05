using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortuga.SceneGraph.Components
{
    public interface IComponentEventListener
    {
        void AddedToGameObject(GameObject gameObject);
        void RemovedFromGameObject(GameObject gameObject);
    }
}
