using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IUtility
    {
        string GetNodePath(XElement? node);
    }
}