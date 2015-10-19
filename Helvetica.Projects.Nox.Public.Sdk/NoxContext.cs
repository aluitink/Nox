using System.Collections.Generic;
using System.Collections.ObjectModel;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;
using Helvetica.Projects.Nox.Public.Sdk.Models;

namespace Helvetica.Projects.Nox.Public.Sdk
{
    public class NoxContext : INoxContext
    {
        public ISpeechSynthesizer SpeechSynthesizer { get; set; }
        public PossiblePhrase Phrase { get; set; }
        public ReadOnlyCollection<KeyValuePair<string, string>> VariableResults { get; set; }
        public ReadOnlyCollection<KeyValuePair<string, string>> DictationResults { get; set; }

        public NoxContext()
        {
            SpeechSynthesizer = new NoxSpeechSynthesizer();
        }
    }
}