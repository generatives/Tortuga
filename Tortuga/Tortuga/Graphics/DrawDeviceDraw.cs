using Tortuga.Drawing.Resources;
using Tortuga.Geometry;
using Tortuga.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;
using System.Linq;

namespace Tortuga.Drawing
{
    public partial class DrawDevice
    {
        private List<Batch> _batches;
        private Dictionary<Texture, ResourceSet> _resourceSets;

        private Vertex[] _vertexArray;
        private DeviceBuffer _vertexBuffer;

        private Vertex[] _rectBuffer = new Vertex[6];

        private uint _length;

        public void Draw(SubTexture texture, RectangleF rect, RgbaFloat color)
        {
            this.Add(GetResourceSet(texture.Texture), rect, texture.TexRect, color);
        }

        public void Draw(SubTexture texture, RectangleF rect)
        {
            this.Add(GetResourceSet(texture.Texture), rect, texture.TexRect, RgbaFloat.White);
        }

        public void Draw(Texture texture, RectangleF uv, RectangleF rect, RgbaFloat color)
        {
            this.Add(GetResourceSet(texture), rect, uv, RgbaFloat.White);
        }

        public void Draw(Texture texture, IList<Vertex> vertices, Matrix3x2 transform)
        {
            this.Add(GetResourceSet(texture), vertices, transform);
        }

        public void Draw(Texture texture, IList<Vertex> vertices)
        {
            this.Add(GetResourceSet(texture), vertices);
        }

        public void Draw(SubTexture texture, Vector2 size, RgbaFloat color, Matrix3x2 transform)
        {
            this.Add(GetResourceSet(texture.Texture), size, texture.TexRect, color, transform);
        }

        public void Draw(SubTexture texture, Vector2 size, Matrix3x2 transform)
        {
            this.Add(GetResourceSet(texture.Texture), size, texture.TexRect, RgbaFloat.White, transform);
        }

        public void Draw(SubTexture texture, Vector2 size, Vector2 position, float rotation)
        {
            this.Add(GetResourceSet(texture.Texture), size, texture.TexRect, RgbaFloat.White, Matrix3x2.CreateRotation(rotation) * Matrix3x2.CreateTranslation(position));
        }

        public void Draw(SubTexture texture, Vector2 size, Vector2 position, float rotation, RgbaFloat color)
        {
            this.Add(GetResourceSet(texture.Texture), size, texture.TexRect, color, Matrix3x2.CreateRotation(rotation) * Matrix3x2.CreateTranslation(position));
        }

        private void Add(ResourceSet textureResourceSet, RectangleF rect, RectangleF texRect, RgbaFloat color)
        {
            EnsureAdditionalSize(6);
            _vertexArray[_length] = new Vertex(new Vector2(rect.Left, rect.Top), color, new Vector2(texRect.Left, texRect.Bottom));
            _vertexArray[_length + 1] = new Vertex(new Vector2(rect.Right, rect.Top), color, new Vector2(texRect.Right, texRect.Bottom));
            _vertexArray[_length + 2] = new Vertex(new Vector2(rect.Left, rect.Bottom), color, new Vector2(texRect.Left, texRect.Top));
            _vertexArray[_length + 3] = new Vertex(new Vector2(rect.Left, rect.Bottom), color, new Vector2(texRect.Left, texRect.Top));
            _vertexArray[_length + 4] = new Vertex(new Vector2(rect.Right, rect.Top), color, new Vector2(texRect.Right, texRect.Bottom));
            _vertexArray[_length + 5] = new Vertex(new Vector2(rect.Right, rect.Bottom), color, new Vector2(texRect.Right, texRect.Top));
            AddBatch(textureResourceSet, 6);
        }

        private void Add(ResourceSet textureResourceSet, Vector2 size, RectangleF texRect, RgbaFloat color, Matrix3x2 transform)
        {
            _rectBuffer[0] = new Vertex(new Vector2(0, 0), color, new Vector2(texRect.Left, texRect.Bottom));
            _rectBuffer[1] = new Vertex(new Vector2(0, size.Y), color, new Vector2(texRect.Left, texRect.Top));
            _rectBuffer[2] = new Vertex(new Vector2(size.X, 0), color, new Vector2(texRect.Right, texRect.Bottom));
            _rectBuffer[3] = new Vertex(new Vector2(0, size.Y), color, new Vector2(texRect.Left, texRect.Top));
            _rectBuffer[4] = new Vertex(new Vector2(size.X, size.Y), color, new Vector2(texRect.Right, texRect.Top));
            _rectBuffer[5] = new Vertex(new Vector2(size.X, 0), color, new Vector2(texRect.Right, texRect.Bottom));
            Add(textureResourceSet, _rectBuffer, transform);
        }

        private void Add(ResourceSet textureResourceSet, IList<Vertex> vertices)
        {
            EnsureAdditionalSize((uint)vertices.Count);
            vertices.CopyTo(_vertexArray, (int)_length);
            AddBatch(textureResourceSet, (uint)vertices.Count);
        }

        private void Add(ResourceSet textureResourceSet, IList<Vertex> vertices, Matrix3x2 transform)
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
            AddBatch(textureResourceSet, (uint)vertices.Count);
        }

        private void AddBatch(ResourceSet textureResourceSet, uint numVertices)
        {
            var lastBatch = _batches.LastOrDefault();
            if(lastBatch.TextureResourceSet != null && lastBatch.TextureResourceSet == textureResourceSet)
            {
                lastBatch.NumVertices += numVertices;
                _batches[_batches.Count - 1] = lastBatch;
            }
            else
            {
                _batches.Add(new Batch()
                {
                    TextureResourceSet = textureResourceSet,
                    NumVertices = numVertices
                });
            }
            _length += numVertices;
        }

        private void DrawBatches()
        {
            GraphicsDevice.UpdateBuffer(_vertexBuffer, (uint)0, ref _vertexArray[0], (uint)(Vertex.SizeInBytes * _length));
            _commandList.SetVertexBuffer(0, _vertexBuffer);

            uint offset = 0;
            foreach (var batch in _batches)
            {
                _commandList.SetGraphicsResourceSet(1, batch.TextureResourceSet);
                _commandList.Draw(batch.NumVertices, 1, offset, 0);
                offset += batch.NumVertices;
            }

            _batches.Clear();
            _length = 0;
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
    }
}
