// See https://aka.ms/new-console-template for more information

namespace Reversers;

internal sealed class Message : IMessage
{
    public bool ShouldRun(IReverser reverser)
    {
        if (reverser.InPath.Equals(reverser.OutPath, StringComparison.OrdinalIgnoreCase))
        {
            WriteInputDirectory(reverser);

            if (reverser.TestRun)
            {
                Console.WriteLine();
                Console.WriteLine("This is a test run. No code files will be changed.");
                Console.WriteLine();
                return true;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine(
                    """
            This program changes all *.cs files found in the input directory and all its sub-directories.
            Do You really want to continue? Y/N
            """
                    );
                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key.HasFlag(ConsoleKey.Y))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Process canceled.");
                    return false;
                }
            }
        }
        else
        {
            WriteInputDirectory(reverser);
            WriteOutputDirectory(reverser);

            if (reverser.TestRun)
            {
                Console.WriteLine("This is a test run. No code files will be written.");
                return true;
            }
            else
            {
                Console.WriteLine("Do You want to continue? Y/N");

                ConsoleKeyInfo key = Console.ReadKey();
                Console.WriteLine();
                if (key.Key.HasFlag(ConsoleKey.Y))
                {
                    return true;
                }
                else
                {
                    Console.WriteLine("Process canceled.");
                    return false;
                }
            }
        }


        static void WriteInputDirectory(IReverser reverser)
        {
            Console.Write("Input Directory: ");
            Console.WriteLine(reverser.InPath);
        }

        static void WriteOutputDirectory(IReverser reverser)
        {
            Console.Write("Output Directory: ");
            Console.WriteLine(reverser.OutPath);
        }
    }
}

