// See https://aka.ms/new-console-template for more information

using CommandLine;
using Reversers;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

const string LogFileName = "Reverser.log";

Parser.Default.ParseArguments<CommandLineArguments>(args)
                   .WithParsed(Run);

static void Run(CommandLineArguments arguments)
{
    using var disposable  = (IDisposable)(Log.Logger = InitLogger(arguments));
    Log.Debug("Logger successfully initialized.");

    var prog = new Reverser(arguments, Log.Logger, new Message());
    prog.Run();
    Console.WriteLine();
    Console.WriteLine("Created log file at {0}", arguments.LogFilePath);

}



static Logger InitLogger(CommandLineArguments arguments)
{
    arguments.LogFilePath = Path.GetFullPath(arguments.LogFilePath);

    arguments.LogFilePath = Path.Combine(Path.GetDirectoryName(arguments.LogFilePath) ?? arguments.LogFilePath, LogFileName);

    return new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(arguments.LogFilePath, 
                          fileSizeLimitBytes: null, 
                          retainedFileCountLimit: null, 
                          restrictedToMinimumLevel: LogEventLevel.Verbose)
            .WriteTo.Console(theme: SystemConsoleTheme.Colored, restrictedToMinimumLevel: (LogEventLevel)arguments.LogLevel)
            .CreateLogger();
}



