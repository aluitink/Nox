using System.Collections.Generic;
using System.Collections.ObjectModel;
using Helvetica.Projects.Nox.Public.Sdk.Models;

namespace Helvetica.Projects.Nox.Public.Sdk.Interfaces
{
    public interface INoxContext
    {
        ServiceContainer ServiceContainer { get; }

        PossiblePhrase Phrase { get; }
        ReadOnlyCollection<KeyValuePair<string, string>> VariableResults { get; }
        ReadOnlyCollection<KeyValuePair<string, string>> DictationResults { get; }
        
    }
}