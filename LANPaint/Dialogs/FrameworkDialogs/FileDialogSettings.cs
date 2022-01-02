namespace LANPaint.Dialogs.FrameworkDialogs;

public abstract class FileDialogSettings
{
    public bool AddExtension { get; set; } = true;
    public bool CheckPathExists { get; set; } = true;
    public string DefaultExt { get; set; } = string.Empty;
    public bool DereferenceLinks { get; set; } = true;
    public string FileName { get; set; } = string.Empty;
    public string[] FileNames { get; set; } = { string.Empty };
    public string Filter { get; set; } = string.Empty;
    public int FilterIndex { get; set; } = 1;
    public string InitialDirectory { get; set; } = string.Empty;
    public string SafeFileName { internal set; get; } = string.Empty;
    public string[] SafeFileNames { internal set; get; } = new string[0];
    public string Title { get; set; } = string.Empty;
    public bool ValidateNames { get; set; } = true;
}