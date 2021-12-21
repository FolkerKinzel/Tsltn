using System;
using System.Collections.Generic;

namespace FolkerKinzel.Tsltn.Controllers
{
    public class UnusedTranslationEventArgs : EventArgs
    {
        public UnusedTranslationEventArgs(IEnumerable<KeyValuePair<long, string>> unusedTranslations)
        {
            this.UnusedTranslations = unusedTranslations;
        }

        public IEnumerable<KeyValuePair<long, string>> UnusedTranslations { get; }

        public IList<long> TranslationsToRemove { get; } = new List<long>();
    }
}
