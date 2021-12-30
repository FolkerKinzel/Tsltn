using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Tsltn.Commands;

public static class BrowseAllTranslationsCommand
{
    public static RoutedCommand MoveUp { get; } = 
        new RoutedCommand("MoveUp", typeof(BrowseAllTranslationsCommand), new InputGestureCollection() { new KeyGesture(Key.Up, ModifierKeys.Control) });

    public static RoutedCommand MoveDown { get; } =
        new RoutedCommand("MoveDown", typeof(BrowseAllTranslationsCommand), new InputGestureCollection() { new KeyGesture(Key.Down, ModifierKeys.Control) });

    public static RoutedCommand CopyText { get; } =
        new RoutedCommand("CopyText", typeof(BrowseAllTranslationsCommand));

}
