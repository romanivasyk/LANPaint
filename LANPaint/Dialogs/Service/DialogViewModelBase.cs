using System;
using System.Collections.Generic;
using System.Text;

namespace LANPaint.Dialogs.Service
{
    public abstract class DialogViewModelBase<TResult>
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public TResult DialogResult { get; set; }

        protected DialogViewModelBase() : this(string.Empty, string.Empty)
        { }
        protected DialogViewModelBase(string title) : this(title, string.Empty)
        { }
        protected DialogViewModelBase(string title, string message)
        {
            Title = title;
            Message = message;
        }

        public void CloseDialogWithResult(IDialogWindow dialog, TResult result)
        {
            DialogResult = result;
            if (dialog != null) dialog.DialogResult = true;
        }
    }
}
