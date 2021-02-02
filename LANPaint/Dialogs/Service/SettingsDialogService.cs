using LANPaint.Views;

namespace LANPaint.Dialogs.Service
{
    public class SettingsDialogService : IDialogService
    {
        public TResult OpenDialog<TResult>(DialogViewModelBase<TResult> viewModel)
        {
            var dialog = new Settings { DataContext = viewModel };
            dialog.ShowDialog();
            return viewModel.DialogResult;
        }
    }
}
