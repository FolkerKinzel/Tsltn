using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn.Commands
{
    public static class TsltnCommand
    {
        public static RoutedCommand Translate { get; } = new RoutedCommand("Translate", typeof(TsltnCommand));
        public static RoutedCommand ChangeSourceDocument { get; } = new RoutedCommand("ChangeSourceDocument", typeof(TsltnCommand));
    }
}
