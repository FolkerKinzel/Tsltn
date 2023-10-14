// See https://aka.ms/new-console-template for more information

using CommandLine;
using Reversers;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

const string LogFileName = "Reverser.log";

Parser.Default.ParseArguments<CommandLineArguments>(args)
                   .WithParsed(Run)
                   .WithNotParsed(HandleCommandLineErrors);

static void Run(CommandLineArguments arguments)
{
    try
    {
        arguments.LogFilePath = InitLogger(arguments);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e);
        return;
    }

    var prog = new Reverser(arguments, Log.Logger);

    //if (o.Verbose)
    //{
    //    Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
    //    Console.WriteLine("Quick Start Example! App is in Verbose mode!");
    //}
    //else
    //{
    //    Console.WriteLine($"Current Arguments: -v {o.Verbose}");
    //    Console.WriteLine("Quick Start Example!");
    //}
}

//static string? InitLogger(CommandLineArguments arguments)
//{
//    if (arguments.LogFilePath is null)
//    {
//        Log.Logger = new LoggerConfiguration()
//            .WriteTo.Console()
//            .CreateLogger();
//        return null;
//    }
//    else
//    {
//        return InitFileLogger(arguments);
//    }
//}

static string InitLogger(CommandLineArguments arguments)
{
    string logFilePath = Path.GetFullPath(arguments.LogFilePath);

    logFilePath = Path.Combine(Path.GetDirectoryName(logFilePath) ?? logFilePath, LogFileName);

    Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(theme: SystemConsoleTheme.Colored, restrictedToMinimumLevel: (LogEventLevel)arguments.LogLevel )
            .WriteTo.File(logFilePath, fileSizeLimitBytes: null, retainedFileCountLimit: null)
            .CreateLogger();

    return logFilePath;
}

static void HandleCommandLineErrors(IEnumerable<Error> errors)
{

}

