using System;

namespace LANPaint.Dialogs.Service;

public class ViewNotFoundException : Exception
{
    public ViewNotFoundException(string message = null, Exception innerException = null) : base(message, innerException)
    { }
}