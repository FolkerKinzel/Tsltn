using FolkerKinzel.Tsltn.Models.Intls;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FolkerKinzel.Tsltn.Models
{
    public sealed class TranslationsController : ITranslation
    {
        private readonly TsltnFile _tsltn;

        internal TranslationsController(TsltnFile tsltn)
        {
            _tsltn = tsltn;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<long, string>> GetAllTranslations() => _tsltn.GetAllTranslations();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveTranslation(long id) => SetTranslation(id, null);


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
