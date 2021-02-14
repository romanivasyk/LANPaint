using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs.MessageBox
{
    public class MessageBoxShow : IMessageBoxShow
    {
        public MessageBoxResult Show(Window owner, string message, string title, MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult, MessageBoxOptions options) =>
            System.Windows.MessageBox.Show(owner, message, title, button, icon, defaultResult, options);
    }
}