using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs.MessageBox
{
    public class MessageBoxSettings
    {
        public MessageBoxButton Button { get; set; } = MessageBoxButton.OK;
        public string Caption { get; set; } = string.Empty;
        public MessageBoxResult DefaultResult { get; set; } = MessageBoxResult.None;
        public MessageBoxImage Icon { get; set; } = MessageBoxImage.None;
        public string Message { get; set; }
        public MessageBoxOptions Options { get; set; } = MessageBoxOptions.None;
    }
}
