using System.Runtime.CompilerServices;

namespace FolkerKinzel.Tsltn.Models.Intls;

internal class KeyValuePairComparer : IEqualityComparer<KeyValuePair<long, string>>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(KeyValuePair<long, string> x, KeyValuePair<long, string> y) 
        => x.Key.Equals(y.Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetHashCode(KeyValuePair<long, string> obj)
        => obj.Key.GetHashCode();
}
