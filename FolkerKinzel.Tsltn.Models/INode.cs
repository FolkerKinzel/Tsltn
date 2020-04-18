using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace FolkerKinzel.Tsltn.Models
{
    public interface INode
    {
        //long ID { get; }
        string InnerText { get; }
        string InnerXml { get; }
        //bool IsManualTranslation { get; }
        INode? NextNode { get; }
        INode? NextUntranslated { get; }
        string NodePath { get; }
        INode? PreviousNode { get; }

        //public Regex WhitespaceRegex { get; }

        INode? FindNode(string nodePathFragment, bool ignoreCase);
        
        string? Translation { get; set; }

    }
}