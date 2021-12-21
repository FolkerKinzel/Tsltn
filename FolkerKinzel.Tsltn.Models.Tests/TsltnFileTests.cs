using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Xml.Linq;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.Tsltn.Models.Tests
{
    [TestClass()]
    public class TsltnFileTests
    {
        [TestMethod()]
        public void XmlSerializationTest()
        {
            var tsltn = new TsltnFile()
            {
                SourceLanguage = "de",
                TargetLanguage = "en",
                SourceDocumentFileName = "Test.xml"
            };

            const string SUMMARY = "summary";


            var navigator = XmlNavigator.Load("test.xml");

            tsltn.SetTranslation(navigator!.GetNodeID(new XElement(SUMMARY, "Hello Car")), "Hallo Auto");

            var auto2 = new XElement(SUMMARY, "Car 2");
            tsltn.SetTranslation(navigator.GetNodeID(auto2), "Auto 2");

            var parent1 = new XElement("Node1", "Hi Manual");
            tsltn.SetTranslation(navigator.GetNodeID(parent1), "Hallo Manual");

            var parent2 = new XElement("Node2", "Manual 2");
            tsltn.SetTranslation(navigator.GetNodeID(parent2), "manuell 2");


            var sb = new StringBuilder();

            var serializer = new XmlSerializer(typeof(TsltnFile));

            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, tsltn);
            }

            string s = sb.ToString();

            using var reader = new StringReader(s);

            var tsltn2 = (TsltnFile)serializer.Deserialize(reader);

          
            Assert.AreEqual("de", tsltn2.SourceLanguage);
            Assert.AreEqual("en", tsltn2.TargetLanguage);

            Assert.IsTrue(tsltn.TryGetTranslation(navigator.GetNodeID(auto2), out string? transl));
            Assert.AreEqual("Auto 2", transl);

            Assert.IsTrue(tsltn.TryGetTranslation(navigator.GetNodeID(parent2), out string? result));
            Assert.AreEqual("manuell 2", result);


        }




    }
}