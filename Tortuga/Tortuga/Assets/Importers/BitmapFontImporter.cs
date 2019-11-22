using Cyotek.Drawing.BitmapFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tortuga.Assets.Importers
{
    public class FontImporter : IImporter
    {
        public object Import(Stream stream)
        {
            var font = new BitmapFont();
            font.Load(stream);
            return font;
        }
    }
}
