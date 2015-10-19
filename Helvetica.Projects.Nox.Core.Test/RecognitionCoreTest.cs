using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Threading;
using Helvetica.Projects.Nox.Public.Sdk;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;

namespace Helvetica.Projects.Nox.Core.Test
{
    [TestClass]
    public class RecognitionCoreTest
    {
        private RecognitionCore _recognitionCore;
        private SpeechRecognitionEngine _recognitionEngine;

        [TestInitialize]
        public void Setup()
        {
            _recognitionEngine = new SpeechRecognitionEngine();
            _recognitionCore = new RecognitionCore("plugins", _recognitionEngine);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _recognitionCore.Dispose();
        }

        [TestMethod, Timeout(5000)]
        public void CanPerformSimpleDictation()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            bool success = false;

            Command builder = new Command();

            builder.Phrase("Search for")
                .Dictation()
                .Handle(context =>
                {
                    success = true;
                    resetEvent.Set();
                });

            _recognitionCore.Add(builder);

            _recognitionEngine.EmulateRecognizeAsync("Search for chicken wranglers");

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(25000)]
        public void CanPerformSimpleCommand()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            bool success = false;

            Command builder = new Command();

            builder
                .Phrase("Test")
                .Handle(context =>
                {
                    success = true;
                    resetEvent.Set();
                });

            _recognitionCore.Add(builder);

            _recognitionEngine.EmulateRecognizeAsync("Test");

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(5000)]
        public void CanPerformSimpleChoice()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            bool success = false;

            Command builder = new Command();
            
            var colorChoices = new ChoiceSet();
            colorChoices
                .Add("red")
                .Add("blue")
                .Add("green");

            builder
                .Phrase("I like the color")
                .Choices("color_choice", colorChoices)
                .Handle(context =>
                {
                    success = true;
                    resetEvent.Set();
                });
            
            _recognitionCore.Add(builder);

            _recognitionEngine.EmulateRecognizeAsync("I like the color red");

            WaitHandle.WaitAll(new[] { resetEvent });
            resetEvent.Reset();

            _recognitionEngine.EmulateRecognizeAsync("I like the color blue");

            WaitHandle.WaitAll(new[] { resetEvent });
            resetEvent.Reset();

            _recognitionEngine.EmulateRecognizeAsync("I like the color green");

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.IsTrue(success);
        }


        [TestMethod, Timeout(5000)]
        public void CanPerformComplexChoice()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            bool success = false;

            Command command = new Command();

            string firstColor = null;
            string secondColor = null;

            var firstColors = new ChoiceSet();
            firstColors
                .Add("red")
                .Add("blue")
                .Add("green");

            var secondColors = new ChoiceSet();
            secondColors
                .Add("yellow")
                .Add("orange")
                .Add("pink");

            command
                .Phrase("I like the colors")
                .Choices("first_color", firstColors)
                .Phrase("and")
                .Choices("second_color", secondColors)
                .Handle(context =>
                {
                    firstColor = context.VariableResults.FirstOrDefault(c => c.Key == "first_color").Value;
                    secondColor = context.VariableResults.FirstOrDefault(c => c.Key == "second_color").Value;
                    resetEvent.Set();
                    success = true;
                });

            _recognitionCore.Add(command);

            _recognitionEngine.EmulateRecognizeAsync("I like the colors red and yellow");

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.AreEqual("red", firstColor);
            Assert.AreEqual("yellow", secondColor);

            resetEvent.Reset();

            _recognitionEngine.EmulateRecognizeAsync("I like the colors blue and orange");

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.AreEqual("blue", firstColor);
            Assert.AreEqual("orange", secondColor);

            resetEvent.Reset();

            _recognitionEngine.EmulateRecognizeAsync("I like the colors green and pink");

            WaitHandle.WaitAll(new[] { resetEvent });
            
            Assert.AreEqual("green", firstColor);
            Assert.AreEqual("pink", secondColor);

            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(5000)]
        public void CanPerformComplexChoiceWithDictation()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            bool success = false;

            Command command = new Command();

            string firstColor = null;
            string secondColor = null;
            string dictation = null;

            var firstColors = new ChoiceSet();
            firstColors
                .Add("red")
                .Add("blue")
                .Add("green");

            var secondColors = new ChoiceSet();
            secondColors
                .Add("yellow")
                .Add("orange")
                .Add("pink");

            command
                .Phrase("I like the colors")
                .Choices("first_color", firstColors)
                .Phrase("and")
                .Choices("second_color", secondColors)
                .Phrase("because")
                .Dictation()
                .Handle(context =>
                {
                    firstColor = context.VariableResults.FirstOrDefault(c => c.Key == "first_color").Value;
                    secondColor = context.VariableResults.FirstOrDefault(c => c.Key == "second_color").Value;
                    dictation = context.VariableResults.FirstOrDefault(c => c.Key == "dictation").Value;
                    resetEvent.Set();
                    success = true;
                });

            _recognitionCore.Add(command);

            string c1 = "red";
            string c2 = "yellow";
            string d1 = "they are pretty";

            string format = string.Format("I like the colors {0} and {1} because {2}", c1, c2, d1);


            _recognitionEngine.EmulateRecognizeAsync(format);

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.AreEqual(c1, firstColor);
            Assert.AreEqual(c2, secondColor);
            Assert.AreEqual(d1, dictation);

            resetEvent.Reset();

            c1 = "blue";
            c2 = "orange";
            d1 = "they clash";

            format = string.Format("I like the colors {0} and {1} because {2}", c1, c2, d1);

            _recognitionEngine.EmulateRecognizeAsync(format);

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.AreEqual(c1, firstColor);
            Assert.AreEqual(c2, secondColor);
            Assert.AreEqual(d1, dictation);

            resetEvent.Reset();

            c1 = "green";
            c2 = "pink";
            d1 = "breast cancer and green bay packers";

            format = string.Format("I like the colors {0} and {1} because {2}", c1, c2, d1);

            _recognitionEngine.EmulateRecognizeAsync(format);

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.AreEqual(c1, firstColor);
            Assert.AreEqual(c2, secondColor);
            Assert.AreEqual(d1, dictation);

            Assert.IsTrue(success);
        }

        [TestMethod, Timeout(5000)]
        public void CanPerformComplexChoiceWithMultipleDictation()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);

            bool success = false;

            Command command = new Command();

            string firstColor = null;
            string secondColor = null;
            string dictation = null;

            var firstColors = new ChoiceSet();
            firstColors
                .Add("red")
                .Add("blue")
                .Add("green");

            var secondColors = new ChoiceSet();
            secondColors
                .Add("yellow")
                .Add("orange")
                .Add("pink");

            command
                .Phrase("I like the colors")
                .Choices("first_color", firstColors)
                .Phrase("and")
                .Choices("second_color", secondColors)
                .Phrase("because")
                .Dictation("d1")
                .Phrase("and other stuff")
                .Dictation("d2")
                .Handle(context =>
                {
                    firstColor = context.VariableResults.FirstOrDefault(c => c.Key == "first_color").Value;
                    secondColor = context.VariableResults.FirstOrDefault(c => c.Key == "second_color").Value;
                    dictation = context.VariableResults.FirstOrDefault(c => c.Key == "dictation").Value;
                    resetEvent.Set();
                    success = true;
                });

            _recognitionCore.Add(command);

            string c1 = "red";
            string c2 = "yellow";
            string d1 = "they are pretty";
            string p1 = "and other stuff";
            string d2 = "blah blah blah";

            string format = string.Format("I like the colors {0} and {1} because {2} {3} {4}", c1, c2, d1, p1, d2);


            _recognitionEngine.EmulateRecognizeAsync(format);

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.AreEqual(c1, firstColor);
            Assert.AreEqual(c2, secondColor);
            Assert.AreEqual(d1, dictation);

            resetEvent.Reset();

            c1 = "blue";
            c2 = "orange";
            d1 = "they clash and other stuff";
            d2 = "yah yah yah";

            format = string.Format("I like the colors {0} and {1} because {2} {3} {4}", c1, c2, d1, p1, d2);

            _recognitionEngine.EmulateRecognizeAsync(format);

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.AreEqual(c1, firstColor);
            Assert.AreEqual(c2, secondColor);
            Assert.AreEqual(d1, dictation);

            resetEvent.Reset();

            c1 = "green";
            c2 = "pink";
            d1 = "breast cancer and green bay packers and other stuff";
            d2 = "yo yo yo";

            format = string.Format("I like the colors {0} and {1} because {2} {3} {4}", c1, c2, d1, p1, d2);

            _recognitionEngine.EmulateRecognizeAsync(format);

            WaitHandle.WaitAll(new[] { resetEvent });

            Assert.AreEqual(c1, firstColor);
            Assert.AreEqual(c2, secondColor);
            Assert.AreEqual(d1, dictation);

            Assert.IsTrue(success);
        }
    }
}
