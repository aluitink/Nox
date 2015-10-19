using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Helvetica.Projects.Nox.Public.Sdk;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;
using Helvetica.Projects.Nox.Public.Sdk.Models;

namespace Helvetica.Projects.Nox.Core
{
    public class RecognitionCore: IDisposable
    {
        protected PluginLoader<INoxCommandSet> CommandSetLoader;

        private readonly SpeechRecognitionEngine _recognitionEngine;
        private readonly Dictionary<Grammar, Action<INoxContext>> _commandActionLookup = new Dictionary<Grammar, Action<INoxContext>>();
        private readonly Dictionary<Grammar, Action<INoxContext>> _systemActionLookup = new Dictionary<Grammar, Action<INoxContext>>();

        private Timer _systemStateTimer = new Timer();

        private bool _systemState = false;

        public RecognitionCore(string pluginDirectoryPath = "plugins", SpeechRecognitionEngine recognitionEngine = null)
        {
             CommandSetLoader = new PluginLoader<INoxCommandSet>(pluginDirectoryPath);

            _recognitionEngine = recognitionEngine ?? new SpeechRecognitionEngine();

            _recognitionEngine.SetInputToDefaultAudioDevice();
            _recognitionEngine.SpeechRecognized += RecognitionEngineOnSpeechRecognized;
            _recognitionEngine.RecognizeCompleted += RecognitionEngineOnRecognizeCompleted;
            _recognitionEngine.SpeechDetected += RecognitionEngineOnSpeechDetected;
            _recognitionEngine.SpeechHypothesized += RecognitionEngineOnSpeechHypothesized;
            _recognitionEngine.SpeechRecognitionRejected += RecognitionEngineOnSpeechRecognitionRejected;  

            _systemStateTimer.Interval = 30000;
            _systemStateTimer.Elapsed += SystemStateTimerOnElapsed;

            if (CommandSetLoader.Plugins != null)
            {
                foreach (INoxCommandSet noxCommandSet in CommandSetLoader.Plugins)
                {
                    LoadCommandSet(noxCommandSet);
                }
            }

            //load systemGrammars

            Command wakeCommand = new Command();
            wakeCommand
                .Phrase("Nox")
                .Handle(context =>
                {
                    context.SpeechSynthesizer.Speak("Yes?");
                    SetSystemState(true);
                });

            Add(wakeCommand, _systemActionLookup);

            Command sleepCommand = new Command();
            sleepCommand
                .Phrase("Go to sleep")
                .Handle(context =>
                {
                    context.SpeechSynthesizer.Speak("You got it boss!");
                    SetSystemState(false);
                });

            Add(sleepCommand, _systemActionLookup);

            sleepCommand = new Command();

            sleepCommand
                .Phrase("I didn't say anything")
                .Handle(context =>
                {
                    context.SpeechSynthesizer.Speak("Very sorry, I'll be here if you need me.");
                    SetSystemState(false);
                });

            Add(sleepCommand);

            Command thanks = new Command();
            thanks
                .Phrase("Thanks")
                .Handle(context =>
                {
                    context.SpeechSynthesizer.Speak("Anything for you sir.");
                });

            Add(thanks);

            thanks = new Command();
            thanks
                .Phrase("Thank you")
                .Handle(context =>
                {
                    context.SpeechSynthesizer.Speak("You're welcome!");
                });

            Add(thanks);

            //Dictation Catchall
            Command dictationCommand = new Command();
            dictationCommand
                .Dictation("dictation_data")
                .Handle(context =>
                {
                    if (_systemState)
                    {
                        var dictation = context.VariableResults.FirstOrDefault(c => c.Key == "dictation_data");
                        context.SpeechSynthesizer.Speak("What?");
                        Console.WriteLine(dictation);
                    }
                });

            Add(dictationCommand, _systemActionLookup);

            SetSystemState(false);
        }

        public void LoadCommandSet(INoxCommandSet commandSet)
        {
            foreach (Command command in commandSet.Commands)
            {
                Add(command);
            }
        }

        public void Add(Command command)
        {
            Add(command, _commandActionLookup);
        }

        public void Start()
        {
            _recognitionEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Stop()
        {
            _recognitionEngine.RecognizeAsyncCancel();
        }

        public void Dispose()
        {
            Stop();
            if(_recognitionEngine != null)
                _recognitionEngine.Dispose();
        }

        protected void Add(Command command, Dictionary<Grammar, Action<INoxContext>> lookup)
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder(); ;

            Action<INoxContext> commandAction = null;

            foreach (Tuple<GrammarType, object> tuple in command.ArgumentList)
            {
                GrammarType grammarType = tuple.Item1;
                object obj = tuple.Item2;
                if (obj == null && grammarType != GrammarType.Dictation) continue;

                switch (grammarType)
                {
                    case GrammarType.Text:
                        string text = obj as string;
                        if (string.IsNullOrWhiteSpace(text))
                            throw new ApplicationException("Value for GrammarType Phrase cannot be Null or Empty");
                        grammarBuilder.Append(text);
                        break;
                    case GrammarType.Choice:
                        var keyValueTuple = obj as Tuple<string, ChoiceSet>;

                        if (keyValueTuple == null)
                            throw new ApplicationException("Value for Choice cannot be Null or Empty");

                        string choicesKey = keyValueTuple.Item1;
                        ChoiceSet choiceSet = keyValueTuple.Item2;

                        if (choiceSet == null)
                            throw new ApplicationException("Value for GrammarType Choice cannot be Null or Empty");

                        Choices choices = new Choices();

                        foreach (Tuple<string, string> choiceKeyValue in choiceSet.ArgumentList)
                        {
                            string key = choiceKeyValue.Item1;
                            string value = choiceKeyValue.Item2;

                            choices.Add(new SemanticResultValue(key, value));
                        }

                        grammarBuilder.Append(new SemanticResultKey(choicesKey, choices));
                        break;
                    case GrammarType.Dictation:
                        string anythingKey = obj as string;
                        if (string.IsNullOrWhiteSpace(anythingKey))
                            throw new ApplicationException("Value for GrammarType Phrase cannot be Null or Empty");

                        var dictation = new GrammarBuilder();
                        dictation.AppendDictation();

                        grammarBuilder.Append(new SemanticResultKey(anythingKey, dictation));
                        break;
                    case GrammarType.Action:
                        var action = obj as Action<INoxContext>;
                        if (action != null)
                            commandAction = action;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            var grammar = new Grammar(grammarBuilder);

            lookup.Add(grammar, commandAction);

            _recognitionEngine.LoadGrammar(grammar);
        }

        protected void InitializeGrammars()
        {
            _recognitionEngine.UnloadAllGrammars();
            _commandActionLookup.Clear();
        }

        protected void SetSystemState(bool active)
        {
            _systemState = active;
            SetCommandGrammarState(active);
            SetSystemStateTimer(active);
        }

        protected void SetAllGrammarState(bool enabled)
        {
            //Enable
            if (enabled)
            {
                if (_systemState) //Only if system is currently active
                    SetCommandGrammarState(true);
                SetSystemGrammarState(true); //Always want system grammar on
            }
            else //Disable All
            {
                SetCommandGrammarState(false);
                SetSystemGrammarState(false);
            }
        }

        protected void SetCommandGrammarState(bool enabled)
        {
            foreach (KeyValuePair<Grammar, Action<INoxContext>> keyValuePair in _commandActionLookup)
            {
                keyValuePair.Key.Enabled = enabled;
            }
        }

        protected void SetSystemGrammarState(bool enabled)
        {
            foreach (KeyValuePair<Grammar, Action<INoxContext>> keyValuePair in _systemActionLookup)
            {
                keyValuePair.Key.Enabled = enabled;
            }
        }

        private void HandleContext(Grammar grammar, INoxContext context)
        {
            Action<INoxContext> action;

            if (_systemActionLookup.TryGetValue(grammar, out action))
            {
                if (action != null)
                {
                    action(context);
                }

                return;
            }
            
            if (!_commandActionLookup.TryGetValue(grammar, out action)) return;

            if (action != null)
            {
                action(context);
                SetSystemStateTimer(true);
            }
            
        }

        private INoxContext BuildContext(SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            INoxContext context = new NoxContext();
            context.VariableResults = GetVariableResults(speechRecognizedEventArgs.Result.Semantics);

            if (speechRecognizedEventArgs.Result != null)
            {
                var result = speechRecognizedEventArgs.Result;

                var retPhrase = new PossiblePhrase();
                retPhrase.Text = result.Text;
                retPhrase.Confidence = result.Confidence;
                retPhrase.Words = GetPossibleWords(result.Words);
                retPhrase.Homophones = GetPossiblePhrase(result.Homophones);
                retPhrase.Alternates = GetPossiblePhrase(result.Alternates);

                context.Phrase = retPhrase;
            }
            return context;
        }

        private void SetSystemStateTimer(bool enabled)
        {
            _systemStateTimer.Stop();
            if(enabled)
                _systemStateTimer.Start();
        }

        private void PrintRecognitionResult(SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            var result = speechRecognizedEventArgs.Result;

            Console.WriteLine("Recognition Result Summary:");
            Console.WriteLine(
                "\tRecognized Phrase: {0}\n" +
                "\tConfidence Score: {1}\n",
                result.Text, result.Confidence);

            Console.WriteLine("Word Summary:");
            foreach (var possibleWord in result.Words)
            {
                Console.WriteLine(
                    "\tText: ({0})\n" +
                    "\tLexical Form: ({1})\n" +
                    "\tPronounciation: ({2})\n" +
                    "\tConfidence: ({3})",
                    possibleWord.Text,
                    possibleWord.LexicalForm,
                    possibleWord.Pronunciation,
                    possibleWord.Confidence);
            }

            Console.WriteLine("Alternate Phrase Collection:");
            foreach (var possiblePhrase in result.Alternates)
            {
                Console.WriteLine(
                "\tPhrase: {0}\n" +
                "\tConfidence Score: {1}\n",
                possiblePhrase.Text, possiblePhrase.Confidence);
            }
        }

        private ReadOnlyCollection<PossibleWord> GetPossibleWords(ReadOnlyCollection<RecognizedWordUnit> words)
        {
            List<PossibleWord> possibleWords = new List<PossibleWord>();
            foreach (RecognizedWordUnit recognizedWordUnit in words)
            {
                PossibleWord word = new PossibleWord();
                word.LexicalForm = recognizedWordUnit.LexicalForm;
                word.Pronunciation = recognizedWordUnit.Pronunciation;
                word.Confidence = recognizedWordUnit.Confidence;
                word.Text = recognizedWordUnit.Text;
                possibleWords.Add(word);
            }
            return possibleWords.AsReadOnly();
        }

        private ReadOnlyCollection<PossiblePhrase> GetPossiblePhrase(ReadOnlyCollection<RecognizedPhrase> phrases)
        {
            List<PossiblePhrase> possiblePhrases = new List<PossiblePhrase>();
            foreach (RecognizedPhrase recognizedPhrase in phrases)
            {
                PossiblePhrase phrase = new PossiblePhrase();
                phrase.Text = recognizedPhrase.Text;
                phrase.Confidence = recognizedPhrase.Confidence;
                phrase.Words = GetPossibleWords(recognizedPhrase.Words);
                //phrase.Homophones = GetPossiblePhrase(recognizedPhrase.Homophones);
                possiblePhrases.Add(phrase);
            }
            return possiblePhrases.AsReadOnly();
        }

        private ReadOnlyCollection<KeyValuePair<string, string>> GetVariableResults(SemanticValue semanticValue)
        {
            if (semanticValue == null)
                return null;
            List<KeyValuePair<string, string>> choiceResults = new List<KeyValuePair<string, string>>();
            foreach (KeyValuePair<string, SemanticValue> keyValuePair in semanticValue)
            {
                choiceResults.Add(new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value.Value.ToString()));
            }

            return choiceResults.AsReadOnly();
        }

        private void RecognitionEngineOnSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs speechRecognitionRejectedEventArgs)
        {
            Console.WriteLine("Rejected");
        }

        private void RecognitionEngineOnSpeechHypothesized(object sender, SpeechHypothesizedEventArgs speechHypothesizedEventArgs)
        {
            Console.Write(".");
        }

        private void RecognitionEngineOnSpeechDetected(object sender, SpeechDetectedEventArgs speechDetectedEventArgs)
        {
            Console.Write("*");
        }

        private void RecognitionEngineOnRecognizeCompleted(object sender, RecognizeCompletedEventArgs recognizeCompletedEventArgs)
        {
            Console.WriteLine("RecognizeCompleted");
        }

        private void RecognitionEngineOnSpeechRecognized(object sender, SpeechRecognizedEventArgs speechRecognizedEventArgs)
        {
            Console.WriteLine();
            PrintRecognitionResult(speechRecognizedEventArgs);
            var context = BuildContext(speechRecognizedEventArgs);
            SetAllGrammarState(false);
            HandleContext(speechRecognizedEventArgs.Result.Grammar, context);
            SetAllGrammarState(true);
        }

        private void SystemStateTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            //Set System State to Inactive
            Console.WriteLine("Going to sleep");
            SetSystemState(false);
        }
    }
}
