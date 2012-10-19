using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S3ToolKit.MagicEngine;

namespace S3ToolKit.MagicEngine.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Engine E = Engine.Instance;

            E.Initialize();
        }
    }
} 
