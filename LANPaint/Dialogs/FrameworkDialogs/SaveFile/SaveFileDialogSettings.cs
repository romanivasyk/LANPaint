namespace LANPaint.Dialogs.FrameworkDialogs.SaveFile;

public class SaveFileDialogSettings : FileDialogSettings
{
    public bool CheckFileExists { get; set; }
    public bool CreatePrompt { get; set; }
    public bool OverwritePrompt { get; set; }
}