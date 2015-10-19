using System.Collections.Generic;
using System.Collections.ObjectModel;
using Helvetica.Projects.Nox.Public.Sdk.Models;

namespace Helvetica.Projects.Nox.Public.Sdk.Interfaces
{
    public interface INoxContext
    {
        ISpeechSynthesizer SpeechSynthesizer { get; set; }
        PossiblePhrase Phrase { get; set; }
        ReadOnlyCollection<KeyValuePair<string, string>> VariableResults { get; set; }
        ReadOnlyCollection<KeyValuePair<string, string>> DictationResults { get; set; }

    }
}