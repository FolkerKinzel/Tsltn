using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn.Commands
{
    public static class TsltnControlCommand
    {
        private static readonly RoutedUICommand _copyText = new RoutedUICommand(Res.CopyText, "CopyText", typeof(TsltnControlCommand));
        private static readonly RoutedUICommand _copyXml = new RoutedUICommand(Res.CopyXml, "CopyXml", typeof(TsltnControlCommand));

        private static readonly RoutedUICommand _browseAll = new RoutedUICommand(Res.BrowseAll, "BrowseAll", typeof(TsltnControlCommand));


        


        public static RoutedUICommand CopyXml
        {
            get { return _copyXml; }
        }

        public static RoutedUICommand CopyText
        {
            get { return _copyText; }
        }

        public static RoutedUICommand BrowseAll
        {
            get { return _browseAll; }
        }
    }
}
