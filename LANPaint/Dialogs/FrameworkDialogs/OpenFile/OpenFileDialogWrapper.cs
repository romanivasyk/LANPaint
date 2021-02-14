using Microsoft.Win32;
using System;
using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs.OpenFile
{
    internal sealed class OpenFileDialogWrapper : IFrameworkDialog
    {
        private readonly OpenFileDialog _dialog;
        private readonly OpenFileDialogSettingsSync _sync;

        public OpenFileDialogWrapper(OpenFileDialogSettings settings)
        {
            _dialog = new OpenFileDialog();
            _sync = new OpenFileDialogSettingsSync(_dialog, settings);
            _sync.ToDialog();
        }

        public bool? ShowDialog(Window owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            var result = _dialog.ShowDialog(owner);
            _sync.ToSettings();
            return result;
        }
    }
}
