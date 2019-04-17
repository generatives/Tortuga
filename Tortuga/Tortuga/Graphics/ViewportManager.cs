using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid;

namespace Tortuga.Graphics
{
    public class ViewportManager
    {
        private int _virtualWidth;
        public int VirtualWidth
        {
            get => _virtualWidth;
            set
            {
                _virtualWidth = value;
                BuildViewport();
            }
        }
        private int _virtualHeight;
        public int VirtualHeight
        {
            get => _virtualHeight;
            set
            {
                _virtualHeight = value;
                BuildViewport();
            }
        }

        private int _windowWidth;
        private int _windowHeight;

        public Viewport Viewport { get; private set; }

        public ViewportManager(int virtualWidth, int virtualHeight)
        {
            _virtualWidth = virtualWidth;
            _virtualHeight = virtualHeight;
            BuildViewport();
        }

        public void WindowChanged(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
            BuildViewport();
        }

        private void BuildViewport()
        {
            var xScale = (float)_windowWidth / VirtualWidth;
            var yScale = (float)_windowHeight / VirtualHeight;

            var scale = xScale < yScale ? xScale : yScale;

            var width = scale * VirtualWidth;
            var height = scale * VirtualHeight;
            var x = (_windowWidth - width) / 2;
            var y = (_windowHeight - height) / 2;

            Viewport = new Viewport(x, y, width, height, 0, 1);
        }

        public Matrix4x4 GetScalingTransform()
        {
            return Matrix4x4.CreateScale(2f / VirtualWidth, 2f / VirtualHeight, 1);
        }
    }
}
