using LANPaint.Dialogs.Service;

namespace LANPaint.Dialogs.Service
{
    public class DialogService : IDialogService
    {
        public T OpenDialog<T>(DialogViewModelBase<T> viewModel)
        {
            var alertView = new DialogWindow { DataContext = viewModel };
            alertView.ShowDialog();
            return viewModel.DialogResult;
        }
    }
}
