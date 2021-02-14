using LANPaint.Dialogs.FrameworkDialogs.MessageBox;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;

namespace LANPaint.Dialogs.FrameworkDialogs
{
    public interface IFrameworkDialogFactory
    {
        IMessageBox CreateMessageBox(MessageBoxSettings settings);
        IFrameworkDialog CreateOpenFileDialog(OpenFileDialogSettings settings);
        IFrameworkDialog CreateSaveFileDialog(SaveFileDialogSettings settings);
    }
}
