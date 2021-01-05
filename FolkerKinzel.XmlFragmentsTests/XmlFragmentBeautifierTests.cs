using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolkerKinzel.XmlFragments;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.XmlFragments.Tests
{
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
}