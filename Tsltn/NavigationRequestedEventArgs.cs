namespace Tsltn;

public class NavigationRequestedEventArgs : EventArgs
{
    public NavigationRequestedEventArgs(string pathFragment, bool caseSensitive, bool wholeWord)
    {
        PathFragment = pathFragment;
        CaseSensitive = caseSensitive;
        WholeWord = wholeWord;
    }

    public string PathFragment { get; }

    public bool CaseSensitive { get; }

    public bool WholeWord { get; }
}
