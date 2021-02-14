using System;
using System.Windows;
using Microsoft.Win32;

namespace LANPaint.Dialogs.FrameworkDialogs.SaveFile
{
    internal sealed class SaveFileDialogWrapper : IFrameworkDialog
    {
        private readonly SaveFileDialog _dialog;
        private readonly SaveFileDialogSettingsSync _sync;

        public SaveFileDialogWrapper(SaveFileDialogSettings settings)
        {
            _dialog = new SaveFileDialog();
            _sync = new SaveFileDialogSettingsSync(_dialog, settings);
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
