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


            XElement root = new XElement(ROOT);
            XElement child = new XElement(CHILD);
            root.Add(new XText(HALLO));
            root.Add(child);

            

            string s = root.GetInnerXml();

            Assert.IsTrue(s.Contains($"<{CHILD} />"));
            Assert.IsTrue(s.Contains(HALLO));
            Assert.IsFalse(s.Contains(ROOT));
        }
    }
}