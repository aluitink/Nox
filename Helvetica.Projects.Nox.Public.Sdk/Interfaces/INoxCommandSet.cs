using System;
using System.Collections.Generic;

namespace Helvetica.Projects.Nox.Public.Sdk.Interfaces
{
    public interface INoxCommandSet
    {
        IEnumerable<Command> Commands { get; set; }
    }
}
