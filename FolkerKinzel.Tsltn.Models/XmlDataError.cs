using FolkerKinzel.Tsltn.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace FolkerKinzel.Tsltn.Models
{
    public class XmlDataError : DataError
    {
        public XmlDataError(INode node, string exceptionMessage) : base(ErrorLevel.Error, $"{Res.InvalidXml}: {exceptionMessage}", node) { }
    }
}
