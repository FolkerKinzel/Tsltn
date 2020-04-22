using System;
using System.Windows;

namespace FolkerKinzel.Tsltn.Controllers
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string message, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult)
        {
            this.Message = message;
            this.Button = button;
            this.Image = image;
            this.DefaultResult = defaultResult;
        }

        public string Message { get; }
        public MessageBoxButton Button { get; }
        public MessageBoxImage Image { get; }
        public MessageBoxResult DefaultResult { get; }

        public MessageBoxResult Result { get; set; }
    }
}