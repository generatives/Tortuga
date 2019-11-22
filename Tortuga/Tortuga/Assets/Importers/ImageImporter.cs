using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tortuga.Assets.Importers
{
    public class ImageImporter : IImporter
    {
        public object Import(Stream stream)
        {
            return Image.Load(stream);
        }
    }
}
