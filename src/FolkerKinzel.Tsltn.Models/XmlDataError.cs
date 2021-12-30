using FolkerKinzel.Tsltn.Models.Resources;

namespace FolkerKinzel.Tsltn.Models;

public class XmlDataError : DataError
{
    public XmlDataError(INode node, string exceptionMessage)
        : base(ErrorLevel.Error, $"{Res.InvalidXml}: {exceptionMessage}", node) { }
}
