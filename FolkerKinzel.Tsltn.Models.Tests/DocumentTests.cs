using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.Tsltn.Models.Tests
{
    [TestClass()]
    public class DocumentTests
    {
#pragma warning disable CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".
        public TestContext TestContext { get; set; }
#pragma warning restore CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".

        

        [TestMethod()]
        public void SaveAsTest()
        {
            string tsltnPath = Path.Combine(TestContext.TestRunResultsDirectory, "test.tsltn");

 
            var doc = Document.Instance;

            doc.CreateNew(TestFiles.TestXml);


            doc.SourceLanguage = "de";
            doc.TargetLanguage = "en";

            //doc.AddAutoTranslation(new XElement("summary", "Hello Car"), "Hallo Auto");

            //doc.AddAutoTranslation(new XElement("summary", "Car 2"), "Auto 2");

            //XText txt1 = new XText("Hi Manual");
            //XElement parent1 = new XElement("Node1", txt1);
            //doc.AddManualTranslation(parent1, "Hallo Manual");

            //XText txt2 = new XText("Manual 2");
            //XElement parent2 = new XElement("Node2", txt2);
            //doc.AddManualTranslation(txt2, "manuell 2");

            //XText txt3 = new XText("Manual 3");
            //parent2.Add(txt3);
            //doc.AddManualTranslation(txt3, "manuell 3");

           
            doc.SaveAs(tsltnPath);

            doc.Open(tsltnPath);

            XElement? section = Document.GetFirstNode();


            //foreach (var nd in node.Document.Root.DescendantNodes())
            //{

            //}

            string s;

            for (int i = 0; i < 20; i++)
            {
                if (section is null)
                {
                    break;
                }

                section = Document.GetNextNode(section);

                s = Utility.GetNodePath(section);
            }

            for (int i = 0; i < 20; i++)
            {
                if (section is null)
                {
                    break;
                }

                section = Document.GetPreviousNode(section);

                s = Utility.GetNodePath(section);

            }

        }

        
    }
}