using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;

namespace LANPaint.Views
{
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Port_PreventInputNonNumbers(object sender, TextCompositionEventArgs e) =>
            e.Handled = !ushort.TryParse(e.Text, out var parsed);

        private void Port_PreventInputSpaces(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space) e.Handled = true;
            else if (e.Key == Key.Enter && OkButton.IsEnabled)
            {
                var peer = new ButtonAutomationPeer(OkButton);
                var invokeProvider = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                invokeProvider?.Invoke();
            }
        }
    }
}