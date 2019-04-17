using Tortuga.Graphics;
using Tortuga.SceneGraph.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tortuga.SceneGraph
{
    public partial class GameObject
    {
        private GameObject _parent;
        public GameObject Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                if(_parent != null)
                {
                    _parent._children.Remove(this);
                }

                if(value != null)
                {
                    value._children.Add(this);
                }

                _parent = value;
            }
        }

        private List<GameObject> _children;
        public IEnumerable<GameObject> Children { get => _children; }
        
        public Scene CurrentScene { get; private set; }

        private Dictionary<Type, Component> _components;
        private List<IUpdateable> _updateables;
        private List<IRenderable> _renderable;
        private List<IComponentEventListener> _componentListeners;
        private List<IGameObjectEventListener> _gameObjectListeners;
        private List<ISceneEventListener> _sceneEventListener;

        public GameObject()
        {
            _children = new List<GameObject>();
            _components = new Dictionary<Type, Component>();
            _updateables = new List<IUpdateable>();
            _renderable = new List<IRenderable>();
            _componentListeners = new List<IComponentEventListener>();
            _gameObjectListeners = new List<IGameObjectEventListener>();
            _sceneEventListener = new List<ISceneEventListener>();
        }

        public void AddChild(GameObject gameObject)
        {
            gameObject.Parent = this;
        }

        public void RemoveChild(GameObject gameObject)
        {
            gameObject.Parent = null;
        }

        public void AddComponent(Component component)
        {
            if (component.GameObject == this) return;

            component.GameObject = this;
            _components[component.GetType()] = component;

            if(component is IUpdateable updateable)
            {
                _updateables.Add(updateable);
            }

            if(component is IRenderable renderable)
            {
                _renderable.Add(renderable);
            }

            if (component is IComponentEventListener componentListener)
            {
                _componentListeners.Add(componentListener);
                componentListener.AddedToGameObject(this);
            }

            if(component is IGameObjectEventListener gameObjectListener)
            {
                _gameObjectListeners.Add(gameObjectListener);
                if(CurrentScene != null)
                {
                    gameObjectListener.AddedToScene(CurrentScene);
                }
            }

            if(component is ISceneEventListener sceneListener)
            {
                _sceneEventListener.Add(sceneListener);
                if(CurrentScene != null && CurrentScene.IsRunning)
                {
                    sceneListener.CurrentSceneStarted();
                }
            }
        }

        public void RemoveComponent(Component component)
        {
            if (component.GameObject != this) return;

            component.GameObject = null;
            _components.Remove(component.GetType());

            if (component is IUpdateable updateable)
            {
                _updateables.Remove(updateable);
            }

            if (component is IRenderable renderable)
            {
                _renderable.Remove(renderable);
            }

            if (component is IComponentEventListener componentListener)
            {
                _componentListeners.Remove(componentListener);
                componentListener.RemovedFromGameObject(this);
            }

            if (component is IGameObjectEventListener gameObjectListener)
            {
                _gameObjectListeners.Remove(gameObjectListener);
            }

            if (component is ISceneEventListener sceneListener)
            {
                _sceneEventListener.Remove(sceneListener);
            }
        }

        public object GetComponent(Type type)
        {
            if(_components.ContainsKey(type))
            {
                return _components[type];
            }
            else
            {
                return null;
            }
        }

        public bool HasComponent(Type type)
        {
            return _components.ContainsKey(type);
        }

        internal void AddedToScene(Scene scene)
        {
            if(CurrentScene != null)
            {
                _gameObjectListeners.ForEach(l => l.RemovedFromScene(scene));
            }

            CurrentScene = scene;

            if(CurrentScene != null)
            {
                _gameObjectListeners.ForEach(l => l.AddedToScene(scene));
                if(CurrentScene.IsRunning)
                {
                    _sceneEventListener.ForEach(l => l.CurrentSceneStarted());
                }
            }
        }

        internal void RemvedFromCurrentScene()
        {
            if (CurrentScene != null)
            {
                _gameObjectListeners.ForEach(l => l.RemovedFromScene(CurrentScene));
            }

            CurrentScene = null;
        }

        internal void SceneStarted()
        {
            _sceneEventListener.ForEach(l => l.CurrentSceneStarted());
        }

        internal void SceneStopped()
        {
            _sceneEventListener.ForEach(l => l.CurrentSceneStopped());
        }

        internal void Update(float time)
        {
            for(int i = 0; i < _updateables.Count; i++)
            {
                _updateables[i].Update(time);
            }
        }

        internal void Render(DrawDevice drawDevice, float time)
        {
            for (int i = 0; i < _renderable.Count; i++)
            {
                _renderable[i].Render(drawDevice, time);
            }
        }
    }
}
