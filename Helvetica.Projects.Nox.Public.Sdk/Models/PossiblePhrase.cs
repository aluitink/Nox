using System.Collections.ObjectModel;
using System.Linq;

namespace Helvetica.Projects.Nox.Public.Sdk.Models
{
    public class PossiblePhrase: Possibility
    {
        public ReadOnlyCollection<PossiblePhrase> Alternates {get; set; }
        public ReadOnlyCollection<PossiblePhrase> Homophones { get; set; }
        public ReadOnlyCollection<PossibleWord> Words { get; set; }

        public override string ToString()
        {
            var words = Words.Select(w => w.Text);
            return string.Join(" ", words);
        }
    }
}