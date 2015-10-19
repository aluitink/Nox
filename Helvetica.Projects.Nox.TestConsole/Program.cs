using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using Helvetica.Projects.Nox.Core;

namespace Helvetica.Projects.Nox.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            RecognitionCore core = new RecognitionCore("E:\\Nox\\plugins");

 
            core.Start();
            
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
            core.Stop();
        }

        
    }
}
