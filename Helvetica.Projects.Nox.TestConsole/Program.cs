using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;
using Helvetica.Projects.Nox.Core;
using Helvetica.Projects.Nox.Public.Sdk;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;
using java.util;

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

    [Export(typeof (INoxCommandSet))]
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

    public class NlpWrapper
    {
        public NlpWrapper()
        {
            var props = new Properties();
            props.setProperty("annotators", "tokenize, ssplit, pos, lemma, ner, parse, dcoref");
            props.setProperty("ner.useSUTime", "0");

            var dir = @"E:\Data\CoreNLP";
            Directory.SetCurrentDirectory(dir);
            var pipeline = new StanfordCoreNLP(props);

            var text = "This is text.";
            
            var annotation = pipeline.process(text);
            
            


            Console.WriteLine(annotation);
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
            core.AddService(new NlpWrapper());
            core.Start();
            
            Console.WriteLine("Press Enter to Exit");
            Console.ReadLine();
            core.Stop();
        }

        
    }
}
