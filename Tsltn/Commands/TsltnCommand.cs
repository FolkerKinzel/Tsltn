using System.Windows.Input;

namespace Tsltn.Commands;

public static class TsltnCommand
{
    public static RoutedCommand Translate { get; } = new RoutedCommand("Translate", typeof(TsltnCommand));
    public static RoutedCommand ChangeSourceDocument { get; } = new RoutedCommand("ChangeSourceDocument", typeof(TsltnCommand));
}
