using LANPaint.Dialogs.FrameworkDialogs.MessageBox;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;

namespace LANPaint.Dialogs.FrameworkDialogs
{
    public interface IFrameworkDialogFactory
    {
        public IMessageBox CreateMessageBox(MessageBoxSettings settings);
        public IFrameworkDialog CreateOpenFileDialog(OpenFileDialogSettings settings);
        public IFrameworkDialog CreateSaveFileDialog(SaveFileDialogSettings settings);
    }
}
