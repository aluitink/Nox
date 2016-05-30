using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helvetica.Projects.Nox.Public.Sdk;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;

namespace Helvetica.Projects.Nox.Plugins.ExamplePlugin
{
    [Export(typeof(INoxCommandSet))]
    public class SearchCommandSet : INoxCommandSet
    {
        public IEnumerable<Command> Commands { get; set; }

        public SearchCommandSet()
        {
            List<Command> commands = new List<Command>();

            Command cmd = new Command();

            ChoiceSet choices = new ChoiceSet();
            choices
                .Add("google")
                .Add("youtube");

            cmd
                .Phrase("Search")
                .Choices("search_provider", choices)
                .Phrase("for")
                .Dictation()
                .Handle(context =>
                {
                    var speechSynthesizer = context.ServiceContainer.GetOrAddService<ISpeechSynthesizer>();
                    var words = context.Phrase.Words.Skip(3);
                    var text = words.Select(w => w.Text);
                    var searchTerm = string.Join(" ", text);

                    var choice = context.VariableResults.FirstOrDefault(c => c.Key == "search_provider");

                    switch (choice.Value)
                    {
                        case "youtube":
                            speechSynthesizer?.Speak("Searching You Tube");
                            SearchYoutube(searchTerm);
                            break;
                        default:
                            speechSynthesizer?.Speak("Searching Google");
                            SearchGoogle(searchTerm);
                            break;
                    }

                    Console.WriteLine(context.Phrase);
                });

            commands.Add(cmd);

            Commands = commands;
        }

        private void SearchGoogle(string t)
        {
            Process.Start("http://google.com/search?q=" + t);
        }
        private void SearchYoutube(string t)
        {
            Process.Start("https://www.youtube.com/results?search_query=" + t);
        }
    }
}
