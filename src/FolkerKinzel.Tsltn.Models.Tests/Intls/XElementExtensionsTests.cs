using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls.Tests;

[TestClass()]
public class XElementExtensionsTests
{
    [TestMethod()]
    public void GetInnerXmlTest()
    {
        const string ROOT = "root";
        const string CHILD = "child";
        const string HALLO = "hallo";


        var root = new XElement(ROOT);
        var child = new XElement(CHILD);
        root.Add(new XText(HALLO));
        root.Add(child);



        string s = root.InnerXml();

        Assert.IsTrue(s.Contains($"<{CHILD} />", StringComparison.Ordinal));
        Assert.IsTrue(s.Contains(HALLO, StringComparison.Ordinal));
        Assert.IsFalse(s.Contains(ROOT, StringComparison.Ordinal));
    }
}
