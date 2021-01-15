namespace LANPaint.Services
{
    public interface IDialogWindowService
    {
        public string OpenFileDialog(string startPath = null);
        public string SaveFileDialog(string startPath = null);
    }
}
