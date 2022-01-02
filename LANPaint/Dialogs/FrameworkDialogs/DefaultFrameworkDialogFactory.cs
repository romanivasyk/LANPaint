using LANPaint.Dialogs.FrameworkDialogs.MessageBox;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;

namespace LANPaint.Dialogs.FrameworkDialogs;

public class DefaultFrameworkDialogFactory : IFrameworkDialogFactory
{
    public virtual IMessageBox CreateMessageBox(MessageBoxSettings settings) => 
        new MessageBoxWrapper(settings);

    public virtual IFrameworkDialog CreateOpenFileDialog(OpenFileDialogSettings settings) =>
        new OpenFileDialogWrapper(settings);

    public virtual IFrameworkDialog CreateSaveFileDialog(SaveFileDialogSettings settings) =>
        new SaveFileDialogWrapper(settings);
}