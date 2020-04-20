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
        public void TsltnFileTest()
        {
            Assert.Fail();
        }


        [TestMethod()]
        public void XmlSerializationTest()
        {
            var tsltn = new TsltnFile
            {
                SourceLanguage = "de",
                TargetLanguage = "en"
            };

            const string SUMMARY = "summary";

            tsltn.SetAutoTranslation(Utility.GetContentHash(new XElement(SUMMARY, "Hello Car"), out string _), "Hallo Auto");

            var auto2 = new XElement(SUMMARY, "Car 2");
            tsltn.SetAutoTranslation(Utility.GetContentHash(auto2, out string _), "Auto 2");

            XElement parent1 = new XElement("Node1", "Hi Manual");
            tsltn.SetTranslation(Utility.GetNodePathHash(parent1), "Hallo Manual");

            XElement parent2 = new XElement("Node2", "Manual 2");
            tsltn.SetTranslation(Utility.GetNodePathHash(parent2), "manuell 2");


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
            Assert.AreEqual("Auto 2", tsltn.GetAutoTranslation(Utility.GetContentHash(auto2, out string _)));
            tsltn.TryGetTranslation(Utility.GetNodePathHash(parent2), out string? result);
            Assert.AreEqual("manuell 2", result);


        }




    }
}