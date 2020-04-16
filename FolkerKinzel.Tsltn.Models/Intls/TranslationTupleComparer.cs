using System;
using System.Collections.Generic;
using System.Text;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal class TranslationTupleComparer : IEqualityComparer<(bool IsManualTranslation, int Hash, string Text)>
    {
        public bool Equals((bool IsManualTranslation, int Hash, string Text) x, (bool IsManualTranslation, int Hash, string Text) y) => 
            x.IsManualTranslation.Equals(y.IsManualTranslation) && x.Hash.Equals(y.Hash);

        public int GetHashCode((bool IsManualTranslation, int Hash, string Text) tpl) => 
            tpl.IsManualTranslation.GetHashCode() ^ tpl.Hash.GetHashCode();
    }
}
