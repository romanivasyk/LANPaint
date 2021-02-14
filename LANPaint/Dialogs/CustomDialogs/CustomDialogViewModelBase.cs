using LANPaint.ViewModels;

namespace LANPaint.Dialogs.CustomDialogs
{
    public abstract class CustomDialogViewModelBase<TResult> : BindableBase
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public TResult Result { get; set; }

        protected CustomDialogViewModelBase() : this(string.Empty, string.Empty)
        { }
        protected CustomDialogViewModelBase(string title) : this(title, string.Empty)
        { }
        protected CustomDialogViewModelBase(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public void CloseDialogWithResult(IDialogWindow dialog, bool? windowDialogResult = false, TResult result = default)
        {
            Result = result;
            if (dialog != null) dialog.DialogResult = windowDialogResult;
        }
    }
}
