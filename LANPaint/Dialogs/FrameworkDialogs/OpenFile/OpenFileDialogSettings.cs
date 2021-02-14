namespace LANPaint.Dialogs.FrameworkDialogs.OpenFile
{
    public class OpenFileDialogSettings : FileDialogSettings
    {
        public bool CheckFileExists { get; set; } = true;
        public bool Multiselect { get; set; }
        public bool ReadOnlyChecked { get; set; }
        public bool ShowReadOnly { get; set; }
    }
}
