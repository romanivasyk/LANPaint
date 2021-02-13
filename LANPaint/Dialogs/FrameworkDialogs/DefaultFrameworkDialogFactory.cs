using LANPaint.Dialogs.FrameworkDialogs.MessageBox;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;
using System;

namespace LANPaint.Dialogs.FrameworkDialogs
{
    public class DefaultFrameworkDialogFactory : IFrameworkDialogFactory
    {
        /// <inheritdoc />
        public virtual IMessageBox CreateMessageBox(MessageBoxSettings settings) => new MessageBoxWrapper(settings);

        /// <inheritdoc />
        public virtual IFrameworkDialog CreateOpenFileDialog(OpenFileDialogSettings settings) => throw new NotImplementedException();

        /// <inheritdoc />
        public virtual IFrameworkDialog CreateSaveFileDialog(SaveFileDialogSettings settings) => throw new NotImplementedException();
    }
}
