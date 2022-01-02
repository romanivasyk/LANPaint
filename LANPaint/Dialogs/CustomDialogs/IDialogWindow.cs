namespace LANPaint.Dialogs.CustomDialogs;

public interface IDialogWindow
{
    public bool? DialogResult { get; set; }
    public object DataContext { get; set; }
    public bool? ShowDialog();
}