using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FolkerKinzel.Tsltn.Models.Tests
{
    [TestClass()]
    public class INodeTests
    {
#pragma warning disable CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".
        public TestContext TestContext { get; set; }
#pragma warning restore CS8618 // Das Non-Nullable-Feld ist nicht initialisiert. Deklarieren Sie das Feld ggf. als "Nullable".


        [TestMethod]
        public void NodeNavigationTests()
        {
            //string tsltnPath = Path.Combine(TestContext.TestRunResultsDirectory, "test.tsltn");

            Document doc = Document.Instance;

            doc.NewTsltn(TestFiles.TestXml);

            INode? node = doc.FirstNode;

            for (int i = 0; i < 10; i++)
            {
                if(node is null)
                {
                    break;
                }

                node = node.GetDescendant();
            }

            for (int i = 0; i < 20; i++)
            {
                if (node is null)
                {
                    break;
                }

                node = node.GetAncestor();
            }

        }
    }
}
