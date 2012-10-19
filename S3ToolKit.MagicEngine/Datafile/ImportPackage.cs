using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3ToolKit.MagicEngine.Datafile
{
    public class ImportPackage : IFileProcess
    {
        public string Filename { get; private set; }

        public ImportPackage(string Filename)
        {
            this.Filename = Filename; 
        }
    }
}
