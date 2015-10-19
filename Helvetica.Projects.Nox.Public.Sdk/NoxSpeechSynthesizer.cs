using System.Speech.Synthesis;
using Helvetica.Projects.Nox.Public.Sdk.Interfaces;

namespace Helvetica.Projects.Nox.Public.Sdk
{
    public class NoxSpeechSynthesizer : ISpeechSynthesizer
    {
        private static readonly SpeechSynthesizer SpeechSynthesizer = new SpeechSynthesizer();
        public void Speak(string text)
        {
            SpeechSynthesizer.Speak(text);
        }
    }
}