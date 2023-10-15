// See https://aka.ms/new-console-template for more information

namespace Reversers;

internal interface IMessage
{
    bool ShouldRun(IReverser reverser);
}