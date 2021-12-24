using FolkerKinzel.Tsltn.Models;
using System.ComponentModel;

namespace FolkerKinzel.Tsltn.Controllers;

public interface IFileController : IDisposable, INotifyPropertyChanged
{
    IDocument? CurrentDocument { get; }
    void CloseCurrentDocument();
    void NewDocument(string xmlFileName);
    void OpenTsltnDocument(string tsltnFileName);
}
