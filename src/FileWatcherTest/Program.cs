using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.IO;
using System.Threading;

namespace FileWatcherTest
{
    internal class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen", Justification = "<Ausstehend>")]
        private static void Main(string[] args)
        {
            const string TEST_DIRECTORY_NAME = "TestDirectory";
            const string ALT_DIRECTORY_NAME = "TestDirectory2";
            const string TEST_FILE_NAME = "TestFile.txt";
            const string OTHER_FILE_NAME = "OtherFile.txt";


            string testDirectoryPath = Path.GetFullPath(TEST_DIRECTORY_NAME);
            string altDirectoryPath = Path.GetFullPath(ALT_DIRECTORY_NAME);


            if (Directory.Exists(testDirectoryPath))
            {
                Directory.Delete(testDirectoryPath, true);
            }

            if (Directory.Exists(altDirectoryPath))
            {
                Directory.Delete(altDirectoryPath, true);
            }

            _ = Directory.CreateDirectory(testDirectoryPath);
            //Directory.CreateDirectory(altDirectoryPath);

            string testFilePath = Path.Combine(testDirectoryPath, TEST_FILE_NAME);
            string otherFilePath = Path.Combine(testDirectoryPath, OTHER_FILE_NAME);


            using FileWatcher fileWatcher = new FileWatcher(testFilePath);

            Console.WriteLine("Create File");
            File.WriteAllText(testFilePath, "");

            Thread.Sleep(1000);

            Console.WriteLine("Change File");
            File.WriteAllText(testFilePath, "Hallo");

            Thread.Sleep(1000);


            Console.WriteLine("Delete File");
            File.Delete(testFilePath);

            Thread.Sleep(1000);


            Console.WriteLine("Recreate File");
            File.WriteAllText(testFilePath, "Hallo");

            Thread.Sleep(1000);


            Console.WriteLine("Rename File");
            File.Move(testFilePath, otherFilePath);

            Thread.Sleep(1000);


            Console.WriteLine("Rename File");
            File.Move(otherFilePath, Path.Combine(testDirectoryPath, "OtherFile2.txt"));

            Thread.Sleep(1000);


            //Console.WriteLine("Move Directory");
            //Directory.Move(testDirectoryPath, altDirectoryPath);


            _ = Console.ReadLine();

        }
    }
}
