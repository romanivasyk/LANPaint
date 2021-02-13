using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs.MessageBox
{
    public interface IMessageBox
    {
        MessageBoxResult Show(Window owner);
    }
}