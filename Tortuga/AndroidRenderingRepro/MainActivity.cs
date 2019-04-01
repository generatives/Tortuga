using Android.App;
using Android.Content.PM;
using Android.OS;
using Tortuga.AndroidPlatform;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Text;
using System.Threading;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;

namespace OpenSkiesAndroidDemo
{
    [Activity(
        MainLauncher = true,
        Label = "OpenSkiesAndroidDemo",
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize
        )]
    public class MainActivity : Activity
    {
        private VeldridSurfaceView surfaceView;
        public GraphicsDevice GraphicsDevice { get; private set; }
        private CommandList _commandList;
        private Pipeline _pipeline;
        private ResourceLayout _textureLayout;
        private ResourceSet _cameraResourceSet;
        private ResourceFactory _factory;

        private DeviceBuffer _transfromBuffer;

        private Vertex[] _vertexArray;
        private DeviceBuffer _vertexBuffer;

        public ResourceSet WhitePixel { get; private set; }
        private const string VertexCode = @"
#version 450
layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec2 TexCoords;
layout(location = 0) out vec4 fsin_Color;
layout(location = 1) out vec2 fsin_texCoords;
layout(set = 0, binding = 0) uniform CameraBuffer
{
    mat4x4 Camera;
};
void main()
{
    gl_Position = Camera * vec4(Position, 0, 1);
    fsin_Color = Color;
    fsin_texCoords = TexCoords;
}";

        private const string FragmentCode = @"
#version 450
layout(location = 0) in vec4 fsin_Color;
layout(location = 1) in vec2 fsin_texCoords;
layout(location = 0) out vec4 fsout_color;
layout(set = 1, binding = 0) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 1) uniform sampler SurfaceSampler;
void main()
{
    fsout_color =  texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords) * fsin_Color;
}";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            bool debug = false;
#if DEBUG
            debug = true;
#endif

            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug,
                PixelFormat.R16_UNorm,
                false,
                ResourceBindingModel.Improved,
                true,
                true);

            GraphicsBackend backend = GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan)
                    ? GraphicsBackend.Vulkan
                    : GraphicsBackend.OpenGLES;
            surfaceView = new VeldridSurfaceView(this, backend, options);
            SetContentView(surfaceView);

            surfaceView.Tick += _window_Tick;
            surfaceView.DeviceCreated += VeldridSurfaceView_DeviceCreated;
            surfaceView.RunContinuousRenderLoop();
        }

        private void VeldridSurfaceView_DeviceCreated()
        {
            GraphicsDevice = surfaceView.GraphicsDevice;
            _factory = GraphicsDevice.ResourceFactory;

            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                        new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4),
                        new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                },
                _factory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main")));

            var cameraBufferDesc = new ResourceLayoutElementDescription(
                "CameraBuffer",
                ResourceKind.UniformBuffer,
                ShaderStages.Vertex);

            var cameraRLD = new ResourceLayoutDescription(cameraBufferDesc);
            var cameraLayout = _factory.CreateResourceLayout(cameraRLD);
            cameraLayout.Name = "Camera Resource Layout";

            _transfromBuffer = _factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer));

            _cameraResourceSet = _factory.CreateResourceSet(
                new ResourceSetDescription(cameraLayout, _transfromBuffer));

            var textureDesc = new ResourceLayoutElementDescription(
                "tex",
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment);
            var samplerDesc = new ResourceLayoutElementDescription(
                "s",
                ResourceKind.Sampler,
                ShaderStages.Fragment);

            var textureRLD = new ResourceLayoutDescription(textureDesc, samplerDesc);
            _textureLayout = _factory.CreateResourceLayout(textureRLD);
            _textureLayout.Name = "Texture Resource Layout";

            // Create pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled: true,
                depthWriteEnabled: true,
                comparisonKind: ComparisonKind.LessEqual);
            pipelineDescription.RasterizerState = new RasterizerStateDescription(
                cullMode: FaceCullMode.Back,
                fillMode: PolygonFillMode.Solid,
                frontFace: FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false);
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new ResourceLayout[] { cameraLayout, _textureLayout };
            pipelineDescription.ShaderSet = shaderSet;
            pipelineDescription.Outputs = surfaceView.MainSwapchain.Framebuffer.OutputDescription;
            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _factory.CreateCommandList();

            _vertexArray = new Vertex[6];
            BufferDescription vbDescription = new BufferDescription(
                (uint)6 * Vertex.SizeInBytes,
                BufferUsage.VertexBuffer);
            _vertexBuffer = _factory.CreateBuffer(vbDescription);

            Image<Rgba32> whitePixel = new Image<Rgba32>(1, 1);
            whitePixel[0, 0] = Rgba32.White;
            var whitePixelTex = new ImageSharpTexture(whitePixel, false);
            var whitePixelTexture = whitePixelTex.CreateDeviceTexture(GraphicsDevice, _factory);

            var _textureView = _factory.CreateTextureView(new TextureViewDescription(whitePixelTexture));
            WhitePixel = _factory.CreateResourceSet(
                new ResourceSetDescription(_textureLayout, _textureView, GraphicsDevice.LinearSampler));
        }

        private void _window_Tick()
        {
            GraphicsDevice.UpdateBuffer(_transfromBuffer, 0, Matrix4x4.CreateScale(1f / surfaceView.Width, 1f / surfaceView.Height, 1f));

            _commandList.Begin();

            _commandList.SetFramebuffer(surfaceView.MainSwapchain.Framebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1f);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0, _cameraResourceSet);

            _vertexArray[0] = new Vertex(new Vector2(0, 400f), RgbaFloat.White, new Vector2(0, 0));
            _vertexArray[1] = new Vertex(new Vector2(400, 400), RgbaFloat.White, new Vector2(1, 0));
            _vertexArray[2] = new Vertex(new Vector2(0, 0), RgbaFloat.White, new Vector2(0, 1));
            _vertexArray[3] = new Vertex(new Vector2(0, 0), RgbaFloat.White, new Vector2(0, 1));
            _vertexArray[4] = new Vertex(new Vector2(400, 400), RgbaFloat.White, new Vector2(1, 0));
            _vertexArray[5] = new Vertex(new Vector2(400, 0), RgbaFloat.White, new Vector2(1, 1));

            GraphicsDevice.UpdateBuffer(_vertexBuffer, (uint)0, ref _vertexArray[0], (uint)(Vertex.SizeInBytes * 6));
            _commandList.SetVertexBuffer(0, _vertexBuffer);

            _commandList.SetGraphicsResourceSet(1, WhitePixel);
            _commandList.Draw(6);

            _commandList.End();
            GraphicsDevice.SubmitCommands(_commandList);
            GraphicsDevice.SwapBuffers(surfaceView.MainSwapchain);
            GraphicsDevice.WaitForIdle();
        }

        protected override void OnPause()
        {
            base.OnPause();
            surfaceView?.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            surfaceView?.OnResume();
        }
    }

    public struct Vertex
    {
        public const uint SizeInBytes = 32;
        public Vector2 Position;
        public RgbaFloat Color;
        public Vector2 UV;

        public Vertex(Vector2 position, RgbaFloat color, Vector2 uv)
        {
            Position = position;
            Color = color;
            UV = uv;
        }
    }
}