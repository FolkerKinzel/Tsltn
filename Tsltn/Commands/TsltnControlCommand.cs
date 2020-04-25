using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn.Commands
{
    public static class TsltnControlCommand
    {
        private static readonly RoutedCommand _copyText = new RoutedCommand("CopyText", typeof(TsltnControlCommand));
        private static readonly RoutedCommand _copyXml = new RoutedCommand("CopyXml", typeof(TsltnControlCommand));

        private static readonly RoutedCommand _browseAll = new RoutedCommand("BrowseAll", typeof(TsltnControlCommand));


        


        public static RoutedCommand CopyXml
        {
            get { return _copyXml; }
        }

        public static RoutedCommand CopyText
        {
            get { return _copyText; }
        }

        public static RoutedCommand BrowseAll
        {
            get { return _browseAll; }
        }
    }
}
