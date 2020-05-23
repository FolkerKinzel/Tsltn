using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn.Commands
{
    public static class TsltnControlCommand
    {
        public static RoutedCommand CopyXml { get; } = new RoutedCommand("CopyXml", typeof(TsltnControlCommand));

        public static RoutedCommand CopyText { get; } = new RoutedCommand("CopyText", typeof(TsltnControlCommand));

        public static RoutedCommand BrowseAll { get; } = new RoutedCommand("BrowseAll", typeof(TsltnControlCommand));

        public static RoutedCommand NextToTranslate { get; } = new RoutedCommand("NextToTranslate", typeof(TsltnControlCommand),
            new InputGestureCollection { new KeyGesture(Key.Right, ModifierKeys.Shift | ModifierKeys.Alt) });
    }
}
