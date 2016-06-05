using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Helvetica.Projects.Nox.Public.Sdk.Interfaces
{
    public interface INoxService
    {
        ReadOnlyCollection<KeyValuePair<string, string>> HandleContext(INoxContext context);
    }

    public interface INoxCommandSet
    {
        IEnumerable<Command> Commands { get; set; }
    }
}
