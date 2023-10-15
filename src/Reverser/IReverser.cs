// See https://aka.ms/new-console-template for more information

using Serilog;

namespace Reversers;
internal interface IReverser
{
    string InPath { get; }
    //ILogger Logger { get; }
    //IMessage Msg { get; }
    string OutPath { get; }
    bool TestRun { get; }
    //string TsltnPath { get; }

    //void Run();
}