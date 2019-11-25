using Cyotek.Drawing.BitmapFont;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Tortuga.Assets;
using Tortuga.DesktopPlatform;
using Tortuga.Graphics;
using Tortuga.Graphics.Text;
using Tortuga.Platform;
using Veldrid;
using Veldrid.StartupUtilities;

namespace Tortoise
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var game = new Game();
            await game.Run();
        }
    }
}
