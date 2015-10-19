using System;
using System.Collections.Generic;

namespace Helvetica.Projects.Nox.Public.Sdk
{
    public class ChoiceSet
    {
        public readonly List<Tuple<string, string>> ArgumentList = new List<Tuple<string, string>>();

        public ChoiceSet Add(string key, string value = null)
        {
            if (value == null)
                value = key;
            ArgumentList.Add(new Tuple<string, string>(key, value));
            return this;
        }
    }
}