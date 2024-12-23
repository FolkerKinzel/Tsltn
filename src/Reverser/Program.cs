// See complete turorial for this application at Reverser.cs

using CommandLine;
using Reversers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

const string LogFileName = "Reverser.log";

Parser.Default.ParseArguments<CommandLineArgument>(args)
                   .WithParsed(Run);

static void Run(CommandLineArgument arguments)
{
    using var disposable  = (IDisposable)(Log.Logger = InitLogger(arguments));
    Log.Debug("Logger successfully initialized.");

    try
    {
        var prog = new Reverser(arguments, Log.Logger, new Message());
        prog.Run();
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, ex.Message);
    }
    Console.WriteLine();
    Console.WriteLine("Created log file at {0}", arguments.LogFilePath);
}



static Logger InitLogger(CommandLineArgument arguments)
{
    arguments.LogFilePath = Path.GetFullPath(arguments.LogFilePath);

    arguments.LogFilePath = Path.Combine(Path.GetDirectoryName(arguments.LogFilePath) ?? arguments.LogFilePath, LogFileName);

    if(File.Exists(arguments.LogFilePath))
    {
        File.Delete(arguments.LogFilePath);
    }

    return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(arguments.LogFilePath, 
                          fileSizeLimitBytes: null, 
                          retainedFileCountLimit: null, 
                          restrictedToMinimumLevel: LogEventLevel.Verbose)
            .WriteTo.Console(theme: SystemConsoleTheme.Colored, restrictedToMinimumLevel: (LogEventLevel)arguments.LogLevel)
            .CreateLogger();
}



