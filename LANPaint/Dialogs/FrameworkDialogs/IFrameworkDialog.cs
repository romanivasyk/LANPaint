using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs;

public interface IFrameworkDialog
{
    public bool? ShowDialog(Window owner);
}