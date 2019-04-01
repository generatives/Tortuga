using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.SPIRV;
using System.Linq;
using Tortuga.Graphics;

namespace OpenSkies.Drawing
{
    public partial class DrawDevice : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        private Swapchain _swapchain;
        private CommandList _commandList;
        private Pipeline _pipeline;
        private ResourceLayout _textureLayout;
        private ResourceSet _cameraResourceSet;
        private ResourceFactory _factory;

        private DeviceBuffer _transfromBuffer;

        public Texture WhitePixel { get; private set; }
        public Texture Grid { get; private set; }

        #region Shaders
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
        #endregion

        public DrawDevice(GraphicsDevice graphics, Swapchain swapchain)
        {
            _batches = new List<Batch>();
            _resourceSets = new Dictionary<Texture, ResourceSet>();

            GraphicsDevice = graphics;
            _swapchain = swapchain;
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
            pipelineDescription.Outputs = _swapchain.Framebuffer.OutputDescription;
            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _factory.CreateCommandList();

            Image<Rgba32> whitePixel = new Image<Rgba32>(1, 1);
            whitePixel[0, 0] = Rgba32.White;
            var whitePixelTex = new ImageSharpTexture(whitePixel, false);
            WhitePixel = whitePixelTex.CreateDeviceTexture(GraphicsDevice, _factory);

            Image<Rgba32> gridImage = new Image<Rgba32>(3, 3);
            gridImage[0, 0] = Rgba32.White;
            gridImage[1, 0] = Rgba32.Red;
            gridImage[2, 0] = Rgba32.White;
            
            gridImage[0, 1] = Rgba32.Yellow;
            gridImage[1, 1] = Rgba32.White;
            gridImage[2, 1] = Rgba32.Green;
            
            gridImage[0, 2] = Rgba32.White;
            gridImage[1, 2] = Rgba32.Blue;
            gridImage[2, 2] = Rgba32.White;
            var gridTex = new ImageSharpTexture(gridImage, false);
            Grid = gridTex.CreateDeviceTexture(GraphicsDevice, _factory);
        }

        private ResourceSet CreateResourceSet(Texture texture)
        {
            var factory = GraphicsDevice.ResourceFactory;
            var _textureView = factory.CreateTextureView(new TextureViewDescription(texture));
            return factory.CreateResourceSet(
                new ResourceSetDescription(_textureLayout, _textureView, GraphicsDevice.PointSampler));
        }

        public void Begin(Matrix4x4 transform)
        {
            GraphicsDevice.UpdateBuffer(_transfromBuffer, 0, transform);

            _commandList.Begin();

            _commandList.SetFramebuffer(_swapchain.Framebuffer);
            _commandList.SetFullViewports();
            _commandList.ClearColorTarget(0, RgbaFloat.Black);
            _commandList.ClearDepthStencil(1f);
            _commandList.SetPipeline(_pipeline);
            _commandList.SetGraphicsResourceSet(0, _cameraResourceSet);
            
            //Draw(WhitePixel, new Rectangle(-(int)info.ScreenSize.X / 2, 0, (int)info.ScreenSize.X, 1));
            //Draw(WhitePixel, new Rectangle(0, -(int)info.ScreenSize.Y / 2, 1, (int)info.ScreenSize.Y));
        }

        public void End()
        {
            DrawBatches();

            _commandList.End();
            GraphicsDevice.SubmitCommands(_commandList);
            GraphicsDevice.SwapBuffers(_swapchain);
            GraphicsDevice.WaitForIdle();
        }

        private ResourceSet GetResourceSet(Texture texture)
        {
            ResourceSet resourceSet;
            if (!_resourceSets.ContainsKey(texture))
            {
                resourceSet = CreateResourceSet(texture);
                _resourceSets[texture] = resourceSet;
            }
            else
            {
                resourceSet = _resourceSets[texture];
            }

            return resourceSet;
        }

        public Texture LoadTexture(string path)
        {
            var source = new ImageSharpTexture(path);
            return source.CreateDeviceTexture(GraphicsDevice, _factory);
        }

        public void Dispose()
        {
            foreach (var SpriteBatch in _resourceSets.Values)
            {
                SpriteBatch.Dispose();
            }
            WhitePixel.Dispose();
            _transfromBuffer.Dispose();
            _textureLayout.Dispose();
            _commandList.Dispose();
            GraphicsDevice.Dispose();
        }
    }
}
