using FolkerKinzel.Tsltn.Models;

namespace Tsltn;

public class DataErrorEventArgs : EventArgs
{
    public DataErrorEventArgs(IEnumerable<DataError> errors) => Errors = errors;

    public IEnumerable<DataError> Errors { get; }
}
