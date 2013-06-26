using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S3ToolKit.MagicEngine.Datafile
{
    public class ImportPackage : IFileProcess
    {
        public string Filename { get; private set; }
        public string ProcessType { get { return "ImportPACKAGE"; } }

        public ImportPackage(string Filename)
        {
            this.Filename = Filename; 
        }

        public override string ToString()
        {
            return string.Format("ImportPackage from {0}", Filename);
        }
    }
}
