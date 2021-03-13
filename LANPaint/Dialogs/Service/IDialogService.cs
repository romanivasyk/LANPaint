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
        public bool? ShowCustomDialog<TResult>(CustomDialogViewModelBase<TResult> viewModel);

        public MessageBoxResult ShowMessageBox(INotifyPropertyChanged ownerViewModel, MessageBoxSettings settings);

        public MessageBoxResult ShowMessageBox(INotifyPropertyChanged ownerViewModel, string message, string caption = "",
            MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None,
            MessageBoxResult defaultResult = MessageBoxResult.None);

        public bool? ShowOpenFileDialog(INotifyPropertyChanged ownerViewModel, OpenFileDialogSettings settings);

        public bool? ShowSaveFileDialog(INotifyPropertyChanged ownerViewModel, SaveFileDialogSettings settings);
    }
}
