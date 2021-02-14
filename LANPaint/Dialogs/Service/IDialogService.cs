using System.ComponentModel;
using LANPaint.Dialogs.FrameworkDialogs.MessageBox;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;
using System.Windows;
using LANPaint.Dialogs.CustomDialogs;

namespace LANPaint.Dialogs.Service
{
    public interface IDialogService
    {
        bool? ShowCustomDialog<TResult>(CustomDialogViewModelBase<TResult> viewModel);

        MessageBoxResult ShowMessageBox(INotifyPropertyChanged ownerViewModel, MessageBoxSettings settings);

        MessageBoxResult ShowMessageBox(INotifyPropertyChanged ownerViewModel, string message, string title = "",
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None);

        bool? ShowOpenFileDialog(INotifyPropertyChanged ownerViewModel, OpenFileDialogSettings settings);

        bool? ShowSaveFileDialog(INotifyPropertyChanged ownerViewModel, SaveFileDialogSettings settings);
    }
}
