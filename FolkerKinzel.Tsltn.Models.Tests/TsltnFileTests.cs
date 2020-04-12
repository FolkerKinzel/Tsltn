using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Xml.Linq;

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

            tsltn.AddAutoTranslation(new XText("Hello Car"), "Hallo Auto");

            XText auto2 = new XText("Car 2");
            tsltn.AddAutoTranslation(auto2, "Auto 2");

            XText txt1 = new XText("Hi Manual");
            XElement parent1 = new XElement("Node1", txt1);
            tsltn.AddManualTranslation(txt1, "Hallo Manual");

            XText txt2 = new XText("Manual 2");
            XElement parent2 = new XElement("Node2", txt2);
            tsltn.AddManualTranslation(txt2, "manuell 2");

            XText txt3 = new XText("Manual 3");
            parent2.Add(txt3);
            tsltn.AddManualTranslation(txt3, "manuell 3");

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
            Assert.AreEqual("Auto 2", tsltn.GetTranslation(auto2));
            Assert.AreEqual("manuell 2", tsltn.GetTranslation(txt2));
            Assert.AreEqual("manuell 3", tsltn.GetTranslation(txt3));


        }




    }
}