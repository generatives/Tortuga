using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tortuga.Assets.Importers
{
    public interface IImporter
    {
        object Import(Stream stream);
    }
}
