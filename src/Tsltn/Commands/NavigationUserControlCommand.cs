﻿using System.Windows.Input;

namespace Tsltn.Commands;

public static class NavigationUserControlCommand
{
    public static RoutedCommand Search { get; }
        = new RoutedCommand("Search", typeof(NavigationUserControlCommand), new InputGestureCollection() { new KeyGesture(Key.Enter) });

}
