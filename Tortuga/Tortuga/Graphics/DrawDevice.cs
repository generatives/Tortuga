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
using Tortuga.Geometry;
using Tortuga.Graphics.Resources;

namespace Tortuga.Graphics
{
    public class DrawDevice : IDisposable
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        private Swapchain _swapchain;
        private CommandList _commandList;
        private Pipeline _pipeline;

        private ResourceLayout _textureLayout;
        private DeviceBuffer _textureSizeBuffer;

        private ResourceSet _cameraResourceSet;
        private ResourceFactory _factory;

        private DeviceBuffer _transfromBuffer;

        private List<Surface> _surfaces;

        public Surface WhitePixel { get; private set; }
        public Surface Grid { get; private set; }

        private List<Batch> _batches;

        private Vertex[] _vertexArray;
        private DeviceBuffer _vertexBuffer;

        private Vertex[] _rectBuffer = new Vertex[6];

        private uint _length;

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
layout(set = 1, binding = 2) uniform TextureSizeBuffer
{
    vec2 TextureSize;
};
void main()
{
    vec2 fraction_texCoords = vec2(fsin_texCoords.x / TextureSize.x, fsin_texCoords.y / TextureSize.y);
    fsout_color =  texture(sampler2D(SurfaceTexture, SurfaceSampler), fraction_texCoords) * fsin_Color;
}";
        #endregion

        public DrawDevice(GraphicsDevice graphics, Swapchain swapchain)
        {
            _batches = new List<Batch>();
            _surfaces = new List<Surface>();
            GraphicsDevice = graphics;
            _swapchain = swapchain;
            _factory = GraphicsDevice.ResourceFactory;

            var vertexDesc = new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main", true);
            var fragDesc = new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main", true);
            var shaders = _factory.CreateFromSpirv(vertexDesc, fragDesc);
            ShaderSetDescription shaderSet = new ShaderSetDescription(
                new[]
                {
                    new VertexLayoutDescription(
                        new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                        new VertexElementDescription("Color", VertexElementSemantic.Color, VertexElementFormat.Float4),
                        new VertexElementDescription("TexCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
                },
                shaders);

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
                "SurfaceTexture",
                ResourceKind.TextureReadOnly,
                ShaderStages.Fragment);
            var samplerDesc = new ResourceLayoutElementDescription(
                "SurfaceSampler",
                ResourceKind.Sampler,
                ShaderStages.Fragment);
            var textureSizeDesc = new ResourceLayoutElementDescription(
                "TextureSizeBuffer",
                ResourceKind.UniformBuffer,
                ShaderStages.Fragment);

            _textureSizeBuffer = _factory.CreateBuffer(new BufferDescription(16, BufferUsage.UniformBuffer));

            var textureRLD = new ResourceLayoutDescription(textureDesc, samplerDesc, textureSizeDesc);
            _textureLayout = _factory.CreateResourceLayout(textureRLD);
            _textureLayout.Name = "Texture Resource Layout";

            // Create pipeline
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = RasterizerStateDescription.Default;
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new ResourceLayout[] { cameraLayout, _textureLayout };
            pipelineDescription.ShaderSet = shaderSet;
            pipelineDescription.Outputs = _swapchain.Framebuffer.OutputDescription;
            _pipeline = _factory.CreateGraphicsPipeline(pipelineDescription);

            _commandList = _factory.CreateCommandList();

            Image<Rgba32> whitePixel = new Image<Rgba32>(1, 1);
            whitePixel[0, 0] = Rgba32.White;

            WhitePixel = CreateSurface(whitePixel);
            
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

            Grid = CreateSurface(gridImage);
        }

        public Surface CreateSurface(Image<Rgba32> image)
        {
            var factory = GraphicsDevice.ResourceFactory;
            var texture = new ImageSharpTexture(image, false);
            var deviceTexture = texture.CreateDeviceTexture(GraphicsDevice, _factory);
            var textureView = factory.CreateTextureView(new TextureViewDescription(deviceTexture));
            var resourceSet = factory.CreateResourceSet(
                new ResourceSetDescription(_textureLayout, textureView, GraphicsDevice.PointSampler, _textureSizeBuffer));
            var material = new Surface(resourceSet, texture.Width, texture.Height);
            _surfaces.Add(material);
            return material;
        }

        public void DestroySurface(Surface surface)
        {
            surface.DisposeResources();
            _surfaces.Remove(surface);
        }

        public void Begin(Matrix4x4 transform, Veldrid.Viewport? viewport = null)
        {
            GraphicsDevice.UpdateBuffer(_transfromBuffer, 0, transform);

            _commandList.Begin();

            _commandList.SetFramebuffer(_swapchain.Framebuffer);

            if(!viewport.HasValue)
            {
                _commandList.SetFullViewports();
            }
            else
            {
                _commandList.SetViewport(0, viewport.Value);
            }

            _commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
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

        private void DrawBatches()
        {
            GraphicsDevice.UpdateBuffer(_vertexBuffer, (uint)0, ref _vertexArray[0], (uint)(Vertex.SizeInBytes * _length));
            _commandList.SetVertexBuffer(0, _vertexBuffer);

            uint offset = 0;
            foreach (var batch in _batches)
            {
                GraphicsDevice.UpdateBuffer(_textureSizeBuffer, 0, new Vector2(batch.Surface.Width, batch.Surface.Height));
                _commandList.SetGraphicsResourceSet(1, batch.Surface.TextureResourceSet);
                _commandList.Draw(batch.NumVertices, 1, offset, 0);
                offset += batch.NumVertices;
            }

            _batches.Clear();
            _length = 0;
        }

        public void Add(Surface surface, RectangleF texRect, RectangleF rect, RgbaFloat color)
        {
            EnsureAdditionalSize(6);
            _vertexArray[_length] = new Vertex(new Vector2(rect.Left, rect.Top), color, new Vector2(texRect.Left, texRect.Bottom));
            _vertexArray[_length + 1] = new Vertex(new Vector2(rect.Right, rect.Top), color, new Vector2(texRect.Right, texRect.Bottom));
            _vertexArray[_length + 2] = new Vertex(new Vector2(rect.Left, rect.Bottom), color, new Vector2(texRect.Left, texRect.Top));
            _vertexArray[_length + 3] = new Vertex(new Vector2(rect.Left, rect.Bottom), color, new Vector2(texRect.Left, texRect.Top));
            _vertexArray[_length + 4] = new Vertex(new Vector2(rect.Right, rect.Top), color, new Vector2(texRect.Right, texRect.Bottom));
            _vertexArray[_length + 5] = new Vertex(new Vector2(rect.Right, rect.Bottom), color, new Vector2(texRect.Right, texRect.Top));
            AddBatch(surface, 6);
        }

        public void Add(Surface surface, RectangleF texRect, Vector2 size, Matrix3x2 transform, RgbaFloat color)
        {
            EnsureAdditionalSize(6);
            _vertexArray[_length] = new Vertex(Vector2.Transform(new Vector2(0, 0), transform), color, new Vector2(texRect.Left, texRect.Top));
            _vertexArray[_length + 1] = new Vertex(Vector2.Transform(new Vector2(0, size.Y), transform), color, new Vector2(texRect.Left, texRect.Bottom));
            _vertexArray[_length + 2] = new Vertex(Vector2.Transform(new Vector2(size.X, 0), transform), color, new Vector2(texRect.Right, texRect.Top));
            _vertexArray[_length + 3] = new Vertex(Vector2.Transform(new Vector2(0, size.Y), transform), color, new Vector2(texRect.Left, texRect.Bottom));
            _vertexArray[_length + 4] = new Vertex(Vector2.Transform(new Vector2(size.X, size.Y), transform), color, new Vector2(texRect.Right, texRect.Bottom));
            _vertexArray[_length + 5] = new Vertex(Vector2.Transform(new Vector2(size.X, 0), transform), color, new Vector2(texRect.Right, texRect.Top));
            AddBatch(surface, 6);
        }

        public void Add(Surface surface, IList<Vertex> vertices)
        {
            EnsureAdditionalSize((uint)vertices.Count);
            vertices.CopyTo(_vertexArray, (int)_length);
            AddBatch(surface, (uint)vertices.Count);
        }

        public void Add(Surface surface, IList<Vertex> vertices, Matrix3x2 transform)
        {
            EnsureAdditionalSize((uint)vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                _vertexArray[_length + i] = new Vertex()
                {
                    Position = Vector2.Transform(vertices[i].Position, transform),
                    Color = vertices[i].Color,
                    UV = vertices[i].UV
                };
            }
            AddBatch(surface, (uint)vertices.Count);
        }

        private void AddBatch(Surface surface, uint numVertices)
        {
            var lastBatch = _batches.LastOrDefault();
            if (lastBatch.Surface != null && lastBatch.Surface == surface)
            {
                lastBatch.NumVertices += numVertices;
                _batches[_batches.Count - 1] = lastBatch;
            }
            else
            {
                _batches.Add(new Batch()
                {
                    Surface = surface,
                    NumVertices = numVertices
                });
            }
            _length += numVertices;
        }

        private void EnsureAdditionalSize(uint size)
        {
            EnsureSize(_length + size);
        }

        private void EnsureSize(uint size)
        {
            if (_vertexArray == null || size > _vertexArray.Length)
            {
                var array = new Vertex[size * 2];
                if (_vertexArray != null)
                {
                    _vertexArray.CopyTo(array, 0);
                }
                _vertexArray = array;
                SetBuffer(_vertexArray.Length);
            }
        }

        private void SetBuffer(int size)
        {
            if (_vertexBuffer != null)
            {
                _vertexBuffer.Dispose();
            }
            var factory = GraphicsDevice.ResourceFactory;
            BufferDescription vbDescription = new BufferDescription(
                (uint)size * Vertex.SizeInBytes,
                BufferUsage.VertexBuffer);
            _vertexBuffer = factory.CreateBuffer(vbDescription);
        }

        public void Dispose()
        {
            foreach(var material in _surfaces)
            {
                material.DisposeResources();
            }
            _surfaces.Clear();
            _cameraResourceSet.Dispose();
            _transfromBuffer.Dispose();
            _textureLayout.Dispose();
            _pipeline.Dispose();
            _commandList.Dispose();
        }
    }
}
