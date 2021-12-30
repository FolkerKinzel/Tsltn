using System.Windows.Input;

namespace Tsltn.Commands;

public static class TsltnControlCommand
{
    public static RoutedCommand CopyXml { get; } = new RoutedCommand("CopyXml", typeof(TsltnControlCommand));

    public static RoutedCommand BrowseAll { get; } = new RoutedCommand("BrowseAll", typeof(TsltnControlCommand));

    public static RoutedCommand NextToTranslate { get; } = new RoutedCommand("NextToTranslate", typeof(TsltnControlCommand),
        new InputGestureCollection { new KeyGesture(Key.Right, ModifierKeys.Shift | ModifierKeys.Alt) });
}
