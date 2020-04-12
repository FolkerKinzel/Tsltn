using System;
using System.Collections.Generic;
using System.Text;
using FolkerKinzel.Strings;

namespace FolkerKinzel.Tsltn.Models
{
    internal static class HashService
    {
        internal static int HashNodePath(string xPath) => xPath.GetStableHashCode(HashType.Ordinal);

        internal static int HashNodePath(StringBuilder xPath) => xPath.GetStableHashCode(HashType.Ordinal);

        internal static int HashOriginalText(string originalText) => originalText.GetStableHashCode(HashType.AlphaNumericNoCase);

    }
}
