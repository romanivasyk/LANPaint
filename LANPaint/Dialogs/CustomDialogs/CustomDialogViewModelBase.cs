using LANPaint.MVVM;

namespace LANPaint.Dialogs.CustomDialogs
{
    public abstract class CustomDialogViewModelBase<TResult> : BindableBase
    {
        public string Caption { get; set; }
        public string Message { get; set; }
        public TResult Result { get; set; }

        protected CustomDialogViewModelBase() : this(string.Empty, string.Empty)
        { }
        protected CustomDialogViewModelBase(string caption) : this(caption, string.Empty)
        { }
        protected CustomDialogViewModelBase(string caption, string message)
        {
            Caption = caption;
            Message = message;
        }

        public void CloseDialogWithResult(IDialogWindow dialog, bool? windowDialogResult = false, TResult result = default)
        {
            Result = result;
            if (dialog != null) dialog.DialogResult = windowDialogResult;
        }
    }
}
