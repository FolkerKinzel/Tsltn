// See https://aka.ms/new-console-template for more information

using Serilog;

namespace Reversers;

internal sealed class Message
{
    internal bool ShouldRun(Reverser reverser)
    {
        if (!reverser.TestRun && reverser.InPath.Equals(reverser.OutPath, StringComparison.OrdinalIgnoreCase))
        {
            WriteInputDirectory(reverser.InPath);
            Console.WriteLine();
            Console.WriteLine(
                """
            This program changes all *.cs files found in the input directory and all its sub-directories.
            Do You really want to continue? Y/N
            """
                );
            ConsoleKeyInfo key = Console.ReadKey();
            if(key.Key.HasFlag(ConsoleKey.Y))
            {
                return true;
            }
            else
            {
                Console.WriteLine("Process canceled.");
                return false;
            }
        }

        return true;

        static void WriteInputDirectory(string inputDirectory)
        {
            Console.Write("Input Directory: ");
            Console.WriteLine(inputDirectory);
        }
    }
}

internal sealed class Reverser
{
    public Reverser(CommandLineArguments arguments, ILogger logger)
    {
        this.Logger = logger;

        TsltnPath = Path.GetFullPath(arguments.TsltnFile);
        InPath = Path.GetFullPath(arguments.InputPath);
        InPath = Path.GetDirectoryName(InPath) ?? InPath;

        if (arguments.OutputPath is null)
        {
            OutPath = InPath;
        }
        else
        {
            OutPath = Path.GetFullPath(arguments.OutputPath);
            OutPath = Path.GetDirectoryName(OutPath) ?? OutPath;
        }

        TestRun = arguments.TestRun;

        Logger.Debug("Reverser initialized.");
    }

    public string TsltnPath { get; }
    public string InPath { get; }
    public string OutPath { get; }
    public bool TestRun { get; }

    public ILogger Logger { get; }

    public void Run()
    {
        
    }
}

