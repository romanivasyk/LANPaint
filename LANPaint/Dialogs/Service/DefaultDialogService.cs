using System;
using System.ComponentModel;
using System.Windows;
using LANPaint.Dialogs.CustomDialogs;
using LANPaint.Dialogs.FrameworkDialogs;
using LANPaint.Dialogs.FrameworkDialogs.MessageBox;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;

namespace LANPaint.Dialogs.Service;

public class DefaultDialogService : IDialogService
{
    private readonly IFrameworkDialogFactory _frameworkDialogFactory;

    public DefaultDialogService(IFrameworkDialogFactory frameworkDialogFactory)
    {
        _frameworkDialogFactory = frameworkDialogFactory ?? throw new ArgumentNullException(nameof(frameworkDialogFactory));
    }

    public bool? ShowCustomDialog<T>(CustomDialogViewModelBase<T> viewModel)
    {
        var customDialog = new CustomDialogWindowShell { DataContext = viewModel };
        return customDialog.ShowDialog();
    }

    public MessageBoxResult ShowMessageBox(INotifyPropertyChanged ownerViewModel, MessageBoxSettings settings)
    {
        if (ownerViewModel == null) throw new ArgumentNullException(nameof(ownerViewModel));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        var messageBox = _frameworkDialogFactory.CreateMessageBox(settings);
        return messageBox.Show(FindOwnerWindow(ownerViewModel));
    }

    public MessageBoxResult ShowMessageBox(INotifyPropertyChanged ownerViewModel, string message, string caption = "", MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None)
    {
        var settings = new MessageBoxSettings
        {
            Message = message,
            Caption = caption,
            Button = button,
            Icon = icon,
            DefaultResult = defaultResult
        };

        return ShowMessageBox(ownerViewModel, settings);
    }

    public bool? ShowOpenFileDialog(INotifyPropertyChanged ownerViewModel, OpenFileDialogSettings settings)
    {
        if (ownerViewModel == null) throw new ArgumentNullException(nameof(ownerViewModel));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        return _frameworkDialogFactory.CreateOpenFileDialog(settings).ShowDialog(FindOwnerWindow(ownerViewModel));
    }

    public bool? ShowSaveFileDialog(INotifyPropertyChanged ownerViewModel, SaveFileDialogSettings settings)
    {
        if (ownerViewModel == null) throw new ArgumentNullException(nameof(ownerViewModel));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        return _frameworkDialogFactory.CreateSaveFileDialog(settings).ShowDialog(FindOwnerWindow(ownerViewModel));
    }

    private Window FindOwnerWindow(INotifyPropertyChanged ownerViewModel)
    {
        return OwnerWindowLocator.GetWindowByBindableDataContext(ownerViewModel) ??
               throw new ViewNotFoundException(
                   $@"Not found any Window where data context is the current instance of '{ownerViewModel.GetType()}'");
    }
}