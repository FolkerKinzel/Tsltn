using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace FolkerKinzel.Tsltn.Models.Tests;

[TestClass()]
public class INodeTests
{
    [NotNull]
    public TestContext? TestContext { get; set; }


    [TestMethod]
    public void NodeNavigationTests()
    {
        string tsltnPath = TestFiles.CreateTsltnFile(TestContext);
        var doc = Document.Load(tsltnPath);

        INode? node = doc.FirstNode;

        for (int i = 0; i < 10; i++)
        {
            if (node is null)
            {
                break;
            }

            node = node.GetDescendant();
        }

        for (int i = 0; i < 20; i++)
        {
            if (node is null)
            {
                break;
            }

            node = node.GetAncestor();
        }

    }
}
