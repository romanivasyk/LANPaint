using System.ComponentModel;
using LANPaint.Dialogs.FrameworkDialogs.MessageBox;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;
using System.Windows;

namespace LANPaint.Dialogs.Service
{
    public interface IDialogService
    {
        bool ShowCustomDialog<TResult>(DialogViewModelBase<TResult> viewModel);
        MessageBoxResult ShowMessageBox(INotifyPropertyChanged ownerViewModel, MessageBoxSettings settings);
        MessageBoxResult ShowMessageBox(INotifyPropertyChanged ownerViewModel, string message, string title = "",
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None);
        bool ShowOpenFileDialog(OpenFileDialogSettings settings);
        bool ShowSaveFileDialog(SaveFileDialogSettings settings);
    }
}
