﻿using System.Windows.Input;

namespace MyComboBoxes;

public static class MyComboBoxCommand
{
    public static RoutedCommand ClearText { get; } = new RoutedCommand("ClearText", typeof(MyComboBoxCommand));
}
