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

        private void Port_PreviewKeyDown(object sender, KeyEventArgs e) => e.Handled = e.Key == Key.Space;
    }
}