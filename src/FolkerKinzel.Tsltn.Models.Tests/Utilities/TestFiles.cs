using FolkerKinzel.Tsltn.Models.Tests.Resources;
using System.IO;


namespace FolkerKinzel.Tsltn.Models.Tests.Utilities
{
    internal static class TestFiles
    {
        
        private const string RESOURCES_DIRECTORY_NAME = "Resources";
        private const string TEST_FILE_DIRECTORY_NAME = "TestFiles";
        private static readonly string _testFileDirectory;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Statische Felder für Referenztyp inline initialisieren", Justification = "<Ausstehend>")]
        static TestFiles()
        {
            ProjectDirectory = Res.ProjDir.Trim();
            _testFileDirectory = Path.Combine(ProjectDirectory, RESOURCES_DIRECTORY_NAME, TEST_FILE_DIRECTORY_NAME);
        }


        internal static string[] GetAll() => Directory.GetFiles(_testFileDirectory);


        internal static string ProjectDirectory { get; }


        internal static string TestXmlPath => Path.Combine(_testFileDirectory, "Test.xml");


        internal static string CreateTsltnFile(TestContext testContext)
        {
            string tsltnPath = Path.Combine(testContext.TestRunResultsDirectory, "test.tsltn");

            var tsltn = new TsltnFile() {
                SourceLanguage = "de",
                TargetLanguage = "en",
                SourceDocumentPath = TestFiles.TestXmlPath
            };

            tsltn.Save(tsltnPath);

            return tsltnPath;
        }

        




    }
}
