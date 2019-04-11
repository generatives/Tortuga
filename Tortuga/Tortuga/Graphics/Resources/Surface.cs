using System;
using System.Collections.Generic;
using System.Text;
using Tortuga.Drawing;
using Veldrid;

namespace Tortuga.Graphics.Resources
{
    public class Surface
    {
        public ResourceSet TextureResourceSet { get; private set; }
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public Surface(ResourceSet textureResourceSet, uint width, uint height)
        {
            TextureResourceSet = textureResourceSet;
            Width = width;
            Height = height;
        }

        public void DisposeResources()
        {
            TextureResourceSet.Dispose();
        }
    }
}
