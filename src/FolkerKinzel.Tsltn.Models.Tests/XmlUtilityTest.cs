using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace FolkerKinzel.Tsltn.Models.Tests;

[TestClass]
public class XmlUtilityTest
{
    [TestMethod]
    public void ContainsPathFragmentTest()
    {
        const string input = "FolkerKinzel.CsvTools.Resources.Res.ResourceManager/summary";

        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "FolkerKinzel", false, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "CsvTools", false, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "Resources", false, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "ResourceManager", false, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "summary", false, true));

        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "FolkerKinzel", true, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "CsvTools", true, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "Resources", true, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "ResourceManager", true, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "summary", true, true));

        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "folkerKinzel", true, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "csvTools", true, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "resources", true, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "resourceManager", true, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "SUMMARY", true, true));

        Assert.IsFalse(XmlUtility.ContainsPathFragment(input, "folkerKinzel", false, true));
        Assert.IsFalse(XmlUtility.ContainsPathFragment(input, "csvTools", false, true));
        Assert.IsFalse(XmlUtility.ContainsPathFragment(input, "resources", false, true));
        Assert.IsFalse(XmlUtility.ContainsPathFragment(input, "resourceManager", false, true));
        Assert.IsFalse(XmlUtility.ContainsPathFragment(input, "SUMMARY", false, true));

        Assert.IsFalse(XmlUtility.ContainsPathFragment(input, "Folker", false, true));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "Folker", false, false));
        Assert.IsFalse(XmlUtility.ContainsPathFragment(input, "folker", false, false));
        Assert.IsTrue(XmlUtility.ContainsPathFragment(input, "folker", true, false));


    }

}
