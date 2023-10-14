// See https://aka.ms/new-console-template for more information

using Serilog;

namespace Reversers;


internal sealed class Reverser
{

    public Reverser(CommandLineArguments arguments, ILogger logger)
    {
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
                          
    }

    

    public string TsltnPath { get; }
    public string InPath { get; }
    public string OutPath { get; }
}

