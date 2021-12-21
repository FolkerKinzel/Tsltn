using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal interface ITranslation
    {
        bool GetHasTranslation(long id);

        void SetTranslation(long nodeID, string? transl);

        bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl);

    }
}
