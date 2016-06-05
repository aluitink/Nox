using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helvetica.Projects.Nox.Public.Sdk;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;

namespace Helvetica.Projects.Nox.Plugins.NatrualLanguageProcessing
{
    [Export(typeof(INoxCommandSet))]
    public class MainCommandSet: INoxCommandSet
    {
        public IEnumerable<Command> Commands { get; set; }

        public MainCommandSet()
        {
            List<Command> commands = new List<Command>();

            Command cmd = new Command();

            cmd
                .Phrase("Analyze")
                .Dictation()
                .Handle(context =>
                {

                });

            commands.Add(cmd);

            Commands = commands;
        }
    }
}
