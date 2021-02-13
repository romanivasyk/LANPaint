using System;

namespace LANPaint.Dialogs.Service
{
    public class ViewNotFoundException : Exception
    {
        public ViewNotFoundException(string message) : base(message)
        { }

        public ViewNotFoundException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
