using System;
using System.Collections.Generic;
using System.Text;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal class KeyValuePairComparer : IEqualityComparer<KeyValuePair<long, string>>
    {
        public bool Equals(KeyValuePair<long, string> x, KeyValuePair<long, string> y)
        {
            return x.Key.Equals(y.Key);
        }

        public int GetHashCode(KeyValuePair<long, string> obj)
        {
            return obj.Key.GetHashCode();
        }
    }
}
