using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helvetica.Projects.Nox.Public.Sdk;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;

namespace Helvetica.Projects.Nox.Plugins.Common
{
    [Export(typeof(INoxCommandSet))]
    public class CommonCommandSet: INoxCommandSet
    {
        public IEnumerable<Command> Commands { get; set; }

        private List<string> _todoList = new List<string>();

        public CommonCommandSet()
        {
            List<Command> commands = new List<Command>();

            Command whatTimeIsIt = new Command();

            whatTimeIsIt
                .Phrase("What time is it?")
                .Handle(SpeakTime);

            commands.Add(whatTimeIsIt);

            whatTimeIsIt = new Command();

            whatTimeIsIt
                .Phrase("What's the time?")
                .Handle(SpeakTime);

            commands.Add(whatTimeIsIt);

            whatTimeIsIt = new Command();

            whatTimeIsIt
                .Phrase("What is the time?")
                .Handle(SpeakTime);

            commands.Add(whatTimeIsIt);
            
            Command whatDayIsIt = new Command();

            whatDayIsIt
                .Phrase("What day is it?")
                .Handle(SpeakDay);

            commands.Add(whatDayIsIt);

            Command todoList = new Command();

            todoList
                .Phrase("I need to")
                .Dictation("task")
                .Handle(StoreTask);

            commands.Add(todoList);

            todoList = new Command();

            todoList
                .Phrase("What do I need to do")
                .Handle(SpeakTasks);

            commands.Add(todoList);

            todoList = new Command();

            todoList
                .Phrase("List tasks")
                .Handle(SpeakTasks);

            commands.Add(todoList);

            Commands = commands;
        }

        private void StoreTask(INoxContext context)
        {
            var dictation = context.VariableResults.FirstOrDefault(c => c.Key == "task");
            var data = dictation.Value;
            _todoList.Add(data);
            context.SpeechSynthesizer.Speak("I'll store that for you.");
        }

        private void SpeakTasks(INoxContext context)
        {
            context.SpeechSynthesizer.Speak("I have the following");
            foreach (string task in _todoList)
            {
                context.SpeechSynthesizer.Speak(task);
            }
        }

        private void SpeakDay(INoxContext context)
        {
            var day = DateTime.Now.DayOfWeek.ToString();
            context.SpeechSynthesizer.Speak(String.Format("{0}", day));
        }

        private void SpeakTime(INoxContext context)
        {
            var time = DateTime.Now.ToString("h:mm");
            context.SpeechSynthesizer.Speak(String.Format("It is {0}", time));
        }
    }
}
