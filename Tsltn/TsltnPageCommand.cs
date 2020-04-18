using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn
{
    public static class TsltnPageCommand
    {
        private static readonly RoutedUICommand _copyText;
        private static readonly RoutedUICommand _copyXml;

        private static readonly RoutedUICommand _browseAll;


        static TsltnPageCommand()
        {
            _copyText = new RoutedUICommand(Res.CopyText, "CopyText", typeof(TsltnPageCommand));
            //_copyText.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift, $"{Res.Cntrl}+{Res.Shift}+C"));

            _copyXml = new RoutedUICommand(Res.CopyXml, "CopyXml", typeof(TsltnPageCommand));

            _browseAll = new RoutedUICommand(Res.BrowseAll, "BrowseAll", typeof(TsltnPageCommand));
        }


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
