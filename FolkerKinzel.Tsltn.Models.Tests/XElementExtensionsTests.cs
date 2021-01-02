using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.Tsltn.Models.Tests
{
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
}