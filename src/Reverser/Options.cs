using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reverser;

internal class Options
{
    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }


    [Value(0, Required = false, HelpText = "Path to the cofiguration file.")]
    public string? ConfigPath { get; set; }


    [Option("m", Default = "production", Required = false, HelpText = "Enables to run in test mode.")]
    public string? Mode { get; set; }
}
