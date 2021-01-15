using Microsoft.Win32;

namespace LANPaint.Services
{
    public class WPFDialogService : IDialogWindowService
    {
        public string OpenFileDialog(string startPath = null)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }

        public string SaveFileDialog(string startPath = null)
        {
            var dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                return dialog.FileName;
            }

            return string.Empty;
        }
    }
}
