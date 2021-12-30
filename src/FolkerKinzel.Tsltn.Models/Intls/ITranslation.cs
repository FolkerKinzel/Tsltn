using System.Diagnostics.CodeAnalysis;

namespace FolkerKinzel.Tsltn.Models.Intls;

internal interface ITranslation
{
    bool GetHasTranslation(long id);

    void SetTranslation(long nodeID, string? transl);

    bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl);

}
