using Tortuga.SceneGraph.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Primitives2D;
using Veldrid;
using Tortuga.Graphics;

namespace Tortuga.SceneGraph
{
    public class Scene
    {
        private List<GameObject> _gameObjects;
        public IEnumerable<GameObject> GameObjects { get => _gameObjects; }

        private List<SceneSystem> _systems;

        public bool IsRunning { get; private set; }

        public Camera Camera { get; private set; }

        public Scene()
        {
            _gameObjects = new List<GameObject>();
            _systems = new List<SceneSystem>();
        }

        public void AddSystem(SceneSystem system)
        {
            if (_systems.Any(s => s.GetType() == system.GetType())) return;

            system.CurrentScene = this;
            _systems.Add(system);
        }

        public void RemoveSystem<T>() where T : SceneSystem
        {
            var system = _systems.FirstOrDefault(s => s is T);
            if(system != null)
            {
                system.CurrentScene = null;
                _systems.Remove(system);
            }
        }

        public void RemoveSystem(SceneSystem system)
        {
            system.CurrentScene = null;
            _systems.Remove(system);
        }

        public void RemoveSystem(Type type)
        {
            var system = _systems.FirstOrDefault(s => s.GetType() == type);
            if (system != null)
            {
                system.CurrentScene = null;
                _systems.Remove(system);
            }
        }

        public T GetSystem<T>() where T : SceneSystem
        {
            return _systems.FirstOrDefault(s => s is T) as T;
        }

        public T GetOrCreateSystem<T>() where T : SceneSystem, new()
        {
            var system = GetSystem<T>();
            if(system == null)
            {
                system = new T();
                AddSystem(system);
            }
            return system;
        }

        public SceneSystem GetSystem(Type type)
        {
            return _systems.FirstOrDefault(s => s.GetType() == type);
        }

        public void AddGameObject(GameObject gameObject)
        {
            if(gameObject.CurrentScene != this)
            {
                if(!gameObject.HasComponent<Transform>())
                {
                    gameObject.AddComponent(new Transform());
                }
                if(gameObject.HasComponent<Camera>())
                {
                    if (Camera == null)
                    {
                        Camera = gameObject.GetComponent<Camera>();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("A camera already exists, ignoring the new camera");
                    }
                }
                _gameObjects.Add(gameObject);
                gameObject.AddedToScene(this);
            }
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            if(gameObject.CurrentScene == this)
            {
                _gameObjects.Remove(gameObject);
                gameObject.RemvedFromCurrentScene();
            }
        }

        public void Update(float time)
        {
            for(int i = 0; i < _gameObjects.Count; i++)
            {
                _gameObjects[i].Update(time);
            }
        }

        public void Render(DrawDevice drawDevice, float time)
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                _gameObjects[i].Render(drawDevice, time);
            }

            drawDevice.DrawLine(-100, 0, 100, 0, RgbaFloat.Red);
            drawDevice.DrawLine(0, -100, 0, 100, RgbaFloat.Red);
        }
    }
}
