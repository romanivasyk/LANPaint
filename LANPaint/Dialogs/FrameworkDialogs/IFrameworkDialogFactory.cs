using LANPaint.Dialogs.FrameworkDialogs.MessageBox;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;
using LANPaint.Dialogs.Service;

namespace LANPaint.Dialogs.FrameworkDialogs
{
    public interface IFrameworkDialogFactory
    {
        IMessageBox CreateMessageBox(MessageBoxSettings settings);

        /// <summary>
        /// Create an instance of the Windows open file dialog.
        /// </summary>
        /// <param name="settings">The settings for the open file dialog.</param>
        IFrameworkDialog CreateOpenFileDialog(OpenFileDialogSettings settings);

        /// <summary>
        /// Create an instance of the Windows save file dialog.
        /// </summary>
        /// <param name="settings">The settings for the save file dialog.</param>
        IFrameworkDialog CreateSaveFileDialog(SaveFileDialogSettings settings);
    }
}
