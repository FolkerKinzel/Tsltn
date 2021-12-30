using FolkerKinzel.XmlFragmentsTests.Resources;
using System.IO;


namespace FolkerKinzel.XmlFragments.Tests.Utilities
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


        internal static string TestXml => Path.Combine(_testFileDirectory, "Test1.txt");

        




    }
}
