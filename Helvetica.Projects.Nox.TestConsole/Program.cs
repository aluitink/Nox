using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Speech.Synthesis;
using Helvetica.Projects.Nox.Core;
using Helvetica.Projects.Nox.Public.Sdk;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;

namespace Helvetica.Projects.Nox.TestConsole
{
    public class TestService
    {
        public event Action<string> ServiceEventFired;

        public void CallEvent(string message)
        {
            OnServiceEventFired(message);   
        }

        protected virtual void OnServiceEventFired(string obj)
        {
            ServiceEventFired?.Invoke(obj);
        }
    }

    [Export(typeof(INoxCommandSet))]
    public class TestServiceCommandSet : INoxCommandSet
    {
        public IEnumerable<Command> Commands { get; set; }

        public TestServiceCommandSet()
        {
            List<Command> commands = new List<Command>();

            Command cmd = new Command();
            cmd
                .Phrase("Call service with message")
                .Dictation()
                .Handle(context =>
                {
                    var service = context.ServiceContainer.GetService<TestService>(true);
                    service.CallEvent(context.Phrase.ToString());
                });

            commands.Add(cmd);

            Commands = commands;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TestService testService = new TestService();
            testService.ServiceEventFired += s =>
            {
                Console.WriteLine("SERVICE: {0}", s);
            };

            RecognitionCore core = new RecognitionCore("E:\\Nox\\plugins");
            core.LoadCommandSet(new TestServiceCommandSet());
            core.AddService<ISpeechSynthesizer>(new NoxSpeechSynthesizer());
            core.AddService<TestService>(testService);
 
            core.Start();
            
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
            core.Stop();
        }

        
    }
}
