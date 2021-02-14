using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs.MessageBox
{
    public class MessageBoxShow : IMessageBoxShow
    {
        public MessageBoxResult Show(Window owner, string message, string caption, MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult, MessageBoxOptions options) =>
            System.Windows.MessageBox.Show(owner, message, caption, button, icon, defaultResult, options);
    }
}