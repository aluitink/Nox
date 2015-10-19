using System;
using System.Collections.Generic;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;
using Helvetica.Projects.Nox.Public.Sdk.Models;

namespace Helvetica.Projects.Nox.Public.Sdk
{
    public class Command
    {
        public readonly List<Tuple<GrammarType, object>> ArgumentList = new List<Tuple<GrammarType, object>>();

        public Command Phrase(string text)
        {
            ArgumentList.Add(new Tuple<GrammarType, object>(GrammarType.Text, text));
            return this;
        }

        public Command Choices(string key, ChoiceSet choiceSet)
        {
            ArgumentList.Add(new Tuple<GrammarType, object>(GrammarType.Choice, new Tuple<string, ChoiceSet>(key, choiceSet)));
            return this;
        }

        public Command Dictation(string key = "dictation")
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ApplicationException("Value for key is required.");

            ArgumentList.Add(new Tuple<GrammarType, object>(GrammarType.Dictation, key));
            return this;
        }

        public Command Handle(Action<INoxContext> handleAction)
        {
            ArgumentList.Add(new Tuple<GrammarType, object>(GrammarType.Action, handleAction));
            return this;
        }
    }
}
