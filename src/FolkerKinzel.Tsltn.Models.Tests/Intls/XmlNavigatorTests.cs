using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls.Tests;

[TestClass()]
public class XmlNavigatorTests
{
    [NotNull()]
    public TestContext? TestContext { get; set; }



    [TestMethod()]
    public void XmlNavigatorTest()
    {
        var nav = XmlNavigator.Load(TestFiles.TestXmlPath);

        Assert.IsNotNull(nav);

        XElement? section = nav!.GetFirstXElement();

        Assert.IsNotNull(section);

        string s;

        for (int i = 0; i < 20; i++)
        {
            if (section is null)
            {
                break;
            }

            section = nav.GetNextXElement(section);

            if (section is null)
            {
                continue;
            }
            _ = nav.GetNodeID(section, out _, out s);

            Assert.IsNotNull(s);
        }

        for (int i = 0; i < 20; i++)
        {
            if (section is null)
            {
                break;
            }

            section = nav.GetPreviousXElement(section);

            if (section is null)
            {
                continue;
            }
            _ = nav.GetNodeID(section, out _, out s);

            Assert.IsNotNull(s);

        }

    }


}
