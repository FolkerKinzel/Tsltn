using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.Tsltn.Models
{
    public partial class Document : IDocument, IFileAccess, ITranslation
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTranslation(long nodeID, string? transl) => _tsltn.SetTranslation(nodeID, transl);


        public bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl)
        {
            if (_tsltn is null)
            {
                transl = null;
                return false;
            }
            else
            {
                return _tsltn.TryGetTranslation(nodeID, out transl);
            }
        }


        public bool GetHasTranslation(long id) => _tsltn.HasTranslation(id);

    }
}
