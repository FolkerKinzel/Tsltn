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
    public class UtilityTests
    {
        [TestMethod()]
        public void TranslateTest()
        {
            

            string test = "<code>code 1</code>Some Text Between<code>code 2</code>";

            var testElement = new XElement("Test");

            testElement.Add(new XText("Hallo"), new XElement("InnerTest"));

            Document.Utility.Translate(testElement, test);

            var clone = Utility.MaskCodeBlock(testElement);
            Document.Utility.Translate(clone, "<code />Dazwischen<code />");
        }
    }
}