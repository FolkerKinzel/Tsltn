using FolkerKinzel.Tsltn.Models.Intls;
using System.Xml.Linq;

namespace FolkerKinzel.XmlFragments.Tests;

[TestClass()]
public class XmlFragmentBeautifierTests
{
    [TestMethod()]
    public void BeautifyTest()
    {
        string innerXml = GetInnerXML();

        innerXml = XmlFragmentBeautifier.Beautify(innerXml);

        Assert.IsNotNull(innerXml);
    }


    private static string GetInnerXML()
    {
        string inner = File.ReadAllText(TestFiles.TestXml);
        var node = XElement.Parse(inner, LoadOptions.PreserveWhitespace);
        return node.InnerXml();
    }
}
