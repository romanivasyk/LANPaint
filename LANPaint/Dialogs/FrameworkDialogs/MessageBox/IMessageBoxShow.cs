using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs.MessageBox;

public interface IMessageBoxShow
{
    public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button,
        MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options);
}