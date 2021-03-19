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

        private void Port_PreventInputSpaces(object sender, KeyEventArgs e) => e.Handled = e.Key == Key.Space;
    }
}