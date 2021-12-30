namespace FolkerKinzel.Tsltn.Models;

public class DataError
{
    public DataError(ErrorLevel level, string message, INode? node)
    {
        Level = level;
        Message = message;
        Node = node;
    }

    public ErrorLevel Level { get; }
    public string Message { get; }
    public INode? Node { get; set; }
}
