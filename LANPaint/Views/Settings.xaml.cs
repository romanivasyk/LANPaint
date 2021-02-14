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

        private void Port_OnPreviewInput(object sender, TextCompositionEventArgs e) =>
            e.Handled = !ushort.TryParse(e.Text, out var parsed);
    }
}
