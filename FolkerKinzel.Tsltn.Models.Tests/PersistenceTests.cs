using Microsoft.VisualStudio.TestTools.UnitTesting;
using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Tests
{
    [TestClass()]
    public class PersistenceTests
    {
#pragma warning disable CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".
        public TestContext TestContext { get; set; }
#pragma warning restore CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".

        

        [TestMethod()]
        public void SaveAsTest()
        {
            string tsltnPath = Path.Combine(TestContext.TestRunResultsDirectory, "test.tsltn");

 

            var data = Document.Instance;

            data.CreateNew(TestFiles.TestXml);


            data.SourceLanguage = "de";
            data.TargetLanguage = "en";
            

            var translation = new Translation("Hello Car", "Hallo Auto");
            data.AddAutoTranslation(translation);

            translation = new Translation("Car 2", "Auto 2");
            data.AddAutoTranslation(translation);

            var manTranslation = new ManualTranslation("Node1");
            manTranslation.AddTranslation(new Translation("Hi Manual", "Hallo Manual"));
            data.AddManualTranslation(manTranslation);


            manTranslation = new ManualTranslation("Node2");
            manTranslation.AddTranslation(new Translation("Manual 2", "Manual 2"));
            data.AddManualTranslation(manTranslation);

            manTranslation = new ManualTranslation("Node2");
            manTranslation.AddTranslation(new Translation("Manual 3", "Manual 3"));
            data.AddManualTranslation(manTranslation);


            data.SaveAs(tsltnPath);

            data.Open(tsltnPath);

            var node = data.GetFirstTextNode();


            //foreach (var nd in node.Document.Root.DescendantNodes())
            //{

            //}

            for (int i = 0; i < 10; i++)
            {
                if (node is null)
                {
                    break;
                }

                node = data.GetNextNode(node);
            }

            for (int i = 0; i < 20; i++)
            {
                if (node is null)
                {
                    break;
                }

                node = data.GetPreviousNode(node);
            }

        }

        
    }
}