namespace FolkerKinzel.Tsltn.Models;

public interface INode
{
    bool HasAncestor { get; }
    bool HasDescendant { get; }

    string InnerXml { get; }
    string NodePath { get; }
    string? Translation { get; set; }

    INode? FindNode(string nodePathFragment, bool ignoreCase, bool wholeWord);
    INode? GetAncestor();
    INode? GetDescendant();
    INode? GetNextUntranslated();
}
