using LANPaint.Dialogs.Service;
using LANPaint.Dialogs.Views;
using LANPaint.ViewModels;

namespace LANPaint.Dialogs.Alerts
{
    public class AlertDialogViewModel : DialogViewModelBase<bool>
    {
        public RelayCommand<IDialogWindow> OkCommand { get; }

        public AlertDialogViewModel(string title, string message) : base(title, message)
        {
            OkCommand = new RelayCommand<IDialogWindow>(OkHandler);
        }

        private void OkHandler(IDialogWindow window) => CloseDialogWithResult(window, true);
    }
}
