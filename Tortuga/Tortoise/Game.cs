using Hyperion;
using Primitives2D;
using SimultaneousCore;
using SimultaneousCore.Entity;
using SimultaneousCore.Simulation;
using SimultaneousLiteNetLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cyotek.Drawing.BitmapFont;
using System.Text;
using System.Threading.Tasks;
using Tortuga.Assets;
using Tortuga.DesktopPlatform;
using Tortuga.Graphics;
using Tortuga.Graphics.Text;
using Tortuga.Platform;
using Veldrid;
using Veldrid.StartupUtilities;
using System.Numerics;

namespace Tortoise
{
    public class Game : ISimultaneousInterface
    {
        private IWindow _window;
        private AssetLoader _assetLoader;
        private BitmapFont font;
        private TextRenderer textRenderer;
        private Stopwatch _frameTimer;
        private double _lastFrameTime;
        private DrawDevice _drawDevice;
        private ViewportManager _viewport;

        bool _renderController = true;
        LiteNetLibNetwork _controllerNet;
        SimultaneousSim _controllerSim;
        PlayerEntity _controllerPlayer;

        bool _renderAuthority = true;
        LiteNetLibNetwork _authorityNet;
        SimultaneousSim _authoritySim;
        public PlayerEntity _authorityPlayer;

        bool _renderObserver = true;
        LiteNetLibNetwork _observerNet;
        SimultaneousSim _observerSim;
        PlayerEntity _observerPlayer;

        public IInputTracker InputTracker => _window.InputTracker;

        public Task Run()
        {
            var platform = new DesktopPlatform();
            _assetLoader = AssetLoader.DefaultAssetLoader(platform);

            WindowCreateInfo wci = new WindowCreateInfo
            {
                X = 100,
                Y = 100,
                WindowWidth = 1280,
                WindowHeight = 720,
                WindowTitle = "Tortuga Demo"
            };

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: true,
                resourceBindingModel: ResourceBindingModel.Improved,
                preferDepthRangeZeroToOne: true,
                preferStandardClipSpaceYDirection: true);
#if DEBUG
            options.Debug = true;
#endif

            _window = platform.CreateWindow(wci, options);
            _window.GraphicsDeviceCreated += LoadResources;
            _window.Tick += Update;
            _window.Resized += _window_Resized;

            _viewport = new ViewportManager(1280, 720);

            _frameTimer = Stopwatch.StartNew();

            _authorityNet = new LiteNetLibNetwork(CreateSerializer());
            _authoritySim = new SimultaneousSim(this, _authorityNet);
            _authoritySim.ListenToPort(5555);
            _authoritySim.ClientSimConnected += _authoritySim_ClientSimConnected;

            _controllerNet = new LiteNetLibNetwork(CreateSerializer());
            _controllerSim = new SimultaneousSim(this, _controllerNet);
            _controllerSim.ConnectToHost("localhost", 5555);

            _observerNet = new LiteNetLibNetwork(CreateSerializer());
            _observerSim = new SimultaneousSim(this, _observerNet);
            _observerSim.ConnectToHost("localhost", 5555);

            return _window.Run();
        }

        private void _window_Resized()
        {
            _viewport.WindowChanged(_window.Width, _window.Height);
        }

        public void LoadResources()
        {
            _drawDevice = new DrawDevice(_window.GraphicsDevice, _window.MainSwapchain);

            font = _assetLoader.LoadFont(BitmapFont.DefaultFontName);
            textRenderer = new TextRenderer(font, _assetLoader, _drawDevice);
        }

        private Serializer CreateSerializer()
        {
            return new Serializer(new SerializerOptions(preserveObjectReferences: true));
        }
        int simsJoined = 0;
        private void _authoritySim_ClientSimConnected(RemoteSim obj)
        {
            simsJoined++;
            if (simsJoined == 2)
            {
                _authorityPlayer = new PlayerEntity(this);
                _authoritySim.NewEntity(_authorityPlayer, _controllerSim.Id, 50);
            }
        }

        protected void Update()
        {
            _lastFrameTime = _frameTimer.Elapsed.TotalSeconds;
            _frameTimer.Restart();

            OnUpdating();

            var vp = _viewport.Viewport;
            _drawDevice.Begin(_viewport.GetScalingTransform(), vp);

            OnDrawing((float)_lastFrameTime);

            _drawDevice.End();

            _window.GraphicsDevice.SwapBuffers(_window.MainSwapchain);
            _window.GraphicsDevice.WaitForIdle();

            //if (_controllerPlayer != null)
            //{
            //    Console.WriteLine($"C- D: {_lastFrameTime}; S: {_controllerSim.GetTimestamp()}; P: X: {_controllerPlayer.Position.X}, Y: {_controllerPlayer.Position.Y}");
            //}

            //if (_authorityPlayer != null)
            //{
            //    Console.WriteLine($"A- D: {_lastFrameTime}; S: {_authoritySim.GetTimestamp()}; P: X: {_authorityPlayer.Position.X}, Y: {_authorityPlayer.Position.Y}");
            //}
        }

        protected void OnUpdating()
        {
            _controllerSim.Update();
            _authoritySim.Update();
            _observerSim.Update();

            if (InputTracker.GetKeyDown(TKey.O))
            {
                _renderObserver = !_renderObserver;
            }
            if (InputTracker.GetKeyDown(TKey.C))
            {
                _renderController = !_renderController;
            }
            if (InputTracker.GetKeyDown(TKey.A))
            {
                _renderAuthority = !_renderAuthority;
            }
        }

        protected void OnDrawing(float time)
        {
            if (_renderController && _controllerPlayer != null)
            {
                _drawDevice.DrawCircle(_controllerPlayer.Position, 10, 9, RgbaFloat.Red);
            }
            if (_renderAuthority && _authorityPlayer != null)
            {
                _drawDevice.DrawCircle(_authorityPlayer.Position, 10, 9, RgbaFloat.Black);
            }
            if (_renderObserver && _observerPlayer != null)
            {
                _drawDevice.DrawCircle(_observerPlayer.Position, 10, 9, RgbaFloat.Yellow);
            }

            textRenderer.DrawText($"FPS: {1f / time}", new Vector2(10, 10));
        }

        public double GetDeltaTime()
        {
            return _lastFrameTime * 1000;
        }

        public IEntityLogic CreateEntity(SimultaneousSim sim, object info)
        {
            var entity = new PlayerEntity(this);
            if (sim == _observerSim)
            {
                _observerPlayer = entity;
            }
            else if (sim == _controllerSim)
            {
                //entity.LogState = true;
                _controllerPlayer = entity;
            }
            return entity;
        }
    }

    public class PlayerEntity : IEntityLogic
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Speed { get; set; } = 5;
        public bool LogState { get; set; }

        private Game _game;

        public PlayerEntity(Game game)
        {
            _game = game;
        }

        public void ApplyDeltas(object deltas)
        {
            var state = (PlayerState)deltas;
            Position = state.Position;
            Velocity = state.Velocity;
        }

        public void ApplySnapshot(object snapshot)
        {
            var state = (PlayerState)snapshot;
            Position = state.Position;
            Velocity = state.Velocity;
        }

        public object GenerateCommands()
        {
            if (LogState)
            {
                Console.WriteLine($"X: {Position.X}, Y: {Position.Y}");
            }
            var keyboard = _game.InputTracker;
            return new PlayerCommand()
            {
                Left = keyboard.GetKey(TKey.Left),
                Right = keyboard.GetKey(TKey.Right),
                Up = keyboard.GetKey(TKey.Up),
                Down = keyboard.GetKey(TKey.Down)
            };
            //return new PlayerCommand()
            //{
            //    Left = false,
            //    Right = true,
            //    Up = false,
            //    Down = true
            //};
        }

        public object GetCreationInfo()
        {
            return 0;
        }

        public void ProcessCommands(IEnumerable<object> commands)
        {
            foreach (var command in commands)
            {
                if (command is PlayerCommand pCommand)
                {
                    var vel = new Vector2();
                    if (pCommand.Left)
                    {
                        vel += new Vector2(-1, 0);
                    }
                    if (pCommand.Right)
                    {
                        vel += new Vector2(1, 0);
                    }
                    if (pCommand.Up)
                    {
                        vel += new Vector2(0, 1);
                    }
                    if (pCommand.Down)
                    {
                        vel += new Vector2(0, -1);
                    }
                    Velocity = vel * Speed;
                }
            }
        }

        public void Simulate(float deltaTime)
        {
            //if (_game._authorityPlayer == this)
            //{
            //    Console.WriteLine("Simulating Authority");
            //}
            //Position += Velocity * (deltaTime / 1000) * 50;
            Position += Velocity;
        }

        public object CalculateDeltas(object oldSnapshot, object newSnapshot)
        {
            return newSnapshot;
        }

        public object TakeSnapshot()
        {
            return new PlayerState() { Position = Position, Velocity = Velocity };
        }

        public class PlayerState
        {
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }

            public override string ToString()
            {
                return $"Px: {Position.X}, Py: {Position.Y}, Vx: {Velocity.X}, Vy: {Velocity.Y}";
            }
        }

        public class PlayerCommand
        {
            public bool Left, Right, Up, Down;

            public override string ToString()
            {
                return $"Left: {Left}, Right: {Right}, Up: {Up}, Down: {Down}";
            }
        }
    }
}