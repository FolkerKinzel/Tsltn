using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace FolkerKinzel.Tsltn.Controllers
{
    public sealed partial class FileController : INotifyPropertyChanged, IFileController
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<ShowFileDialogEventArgs>? ShowFileDialog;
        public event EventHandler<MessageEventArgs>? Message;
        public event EventHandler? RefreshData;
        //public event EventHandler<HasContentChangedEventArgs>? HasContentChanged;
        //public event EventHandler<NewFileNameEventArgs>? NewFileName;
        public event EventHandler<BadFileNameEventArgs>? BadFileName;


        private void OnPropertyChanged([CallerMemberName] string propName = "" ) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private void OnFileDialog(ShowFileDialogEventArgs e) => ShowFileDialog?.Invoke(this, e);

        private void OnRefreshData() => RefreshData?.Invoke(this, EventArgs.Empty);

        //private void OnNewFileName(string fileName) => this.NewFileName?.Invoke(this, new NewFileNameEventArgs(fileName));

        private void OnBadFileName(string fileName) => this.BadFileName?.Invoke(this, new BadFileNameEventArgs(fileName));


        //private void OnHasContentChanged(bool hasContent) => this.HasContentChanged?.Invoke(this, new HasContentChangedEventArgs(hasContent));

        private void OnMessage(MessageEventArgs args) => this.Message?.Invoke(this, args);
    }
}
