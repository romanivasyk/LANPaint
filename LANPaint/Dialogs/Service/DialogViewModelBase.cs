using LANPaint.Dialogs.Views;
using LANPaint.ViewModels;

namespace LANPaint.Dialogs.Service
{
    public abstract class DialogViewModelBase<TResult> : BindableBase
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public TResult Result { get; set; }

        protected DialogViewModelBase() : this(string.Empty, string.Empty)
        { }
        protected DialogViewModelBase(string title) : this(title, string.Empty)
        { }
        protected DialogViewModelBase(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public void CloseDialogWithResult(IDialogWindow dialog, TResult result = default)
        {
            Result = result;
            if (dialog != null) dialog.DialogResult = true;
        }
    }
}
