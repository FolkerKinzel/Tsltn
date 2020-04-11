using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;

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

            var translation = new Translation("Hello Car", "Hallo Auto");
            tsltn.AddAutoTranslation(translation);

            translation = new Translation("Car 2", "Auto 2");
            tsltn.AddAutoTranslation(translation);

            var manTranslation = new ManualTranslation("Node1");
            manTranslation.AddTranslation(new Translation("Hi Manual", "Hallo Manual"));
            tsltn.AddManualTranslation(manTranslation);

          
            manTranslation = new ManualTranslation("Node2");
            manTranslation.AddTranslation(new Translation("Manual 2", "Manual 2"));
            tsltn.AddManualTranslation(manTranslation);

            manTranslation = new ManualTranslation("Node2");
            manTranslation.AddTranslation(new Translation("Manual 3", "Manual 3"));
            tsltn.AddManualTranslation(manTranslation);

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
            Assert.AreEqual("Auto 2", tsltn.GetTranslation("Car 2", "nix da")?.Value);
            Assert.AreEqual("Manual 2", tsltn.GetTranslation("Manual 2", "Node2")?.Value);
            Assert.AreEqual("Manual 3", tsltn.GetTranslation("Manual 3", "Node2")?.Value);


        }




    }
}