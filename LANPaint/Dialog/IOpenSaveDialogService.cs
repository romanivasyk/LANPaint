namespace LANPaint.Dialog
{
    public interface IOpenSaveDialogService
    {
        public string OpenFileDialog(string startPath = null);
        public string SaveFileDialog(string startPath = null);
    }
}
