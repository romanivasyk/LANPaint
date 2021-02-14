using System;
using System.Windows;

namespace LANPaint.Dialogs.FrameworkDialogs.MessageBox
{
    public class MessageBoxWrapper : IMessageBox
    {
        private readonly IMessageBoxShow _messageBoxShow;
        private readonly MessageBoxSettings _settings;

        public MessageBoxWrapper(IMessageBoxShow messageBoxShow, MessageBoxSettings messageBoxSettings)
        {
            _messageBoxShow = messageBoxShow ?? throw new ArgumentNullException(nameof(messageBoxShow));
            _settings = messageBoxSettings ?? throw new ArgumentNullException(nameof(messageBoxSettings));
        }

        public MessageBoxWrapper(MessageBoxSettings messageBoxSettings) : this(new MessageBoxShow(), messageBoxSettings)
        { }

        public MessageBoxResult Show(Window owner)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));

            return _messageBoxShow.Show(owner, _settings.Message, _settings.Caption, _settings.Button, _settings.Icon,
                _settings.DefaultResult, _settings.Options);
        }
    }
}
