using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reversers;

public class CommandLineArguments
{
    //[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    //public bool Verbose { get; set; }


    [Option("tsltn",  Required = true, HelpText = "Path to the TSLTN file.")]
    [NotNull]
    public string? TsltnFile { get; set; }


    [Option("in", Required = true, HelpText = "Path to the input directory.")]
    [NotNull]
    public string? InputPath { get; set; }


    [Option("out", Required = false, HelpText = "Path to the output directory.")]
    public string? OutputPath { get; set; }


    [Option("loglevel", Required = false, HelpText = "The log level.")]
    public string? LogLevel { get; set; }


    [Option("log", Required = false, HelpText = "The log level.")]
    public string? LogFilePath { get; set; }


    [Option("test", Default = "false", Required = false, HelpText = "Enables to run in test mode.")]
    public bool Test { get; set; }
}
