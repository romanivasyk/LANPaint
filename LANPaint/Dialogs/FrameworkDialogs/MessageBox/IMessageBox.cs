using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs.MessageBox
{
    public interface IMessageBox
    {
        public MessageBoxResult Show(Window owner);
    }
}