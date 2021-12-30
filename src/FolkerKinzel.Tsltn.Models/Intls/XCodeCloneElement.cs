using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls;

internal class XCodeCloneElement : XElement
{
    private static readonly XElement _emptyCodeBlock = new(Sandcastle.CODE);

    internal XCodeCloneElement(XElement source) : base(source)
    {
        Source = source;

        foreach (XElement codeNode in Elements(Sandcastle.CODE).ToArray())
        {
            codeNode.ReplaceWith(_emptyCodeBlock);
        }
    }

    internal XElement Source { get; }
}
