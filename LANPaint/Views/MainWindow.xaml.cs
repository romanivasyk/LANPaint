using LANPaint.DialogServices;
using LANPaint.Services.UDP;
using LANPaint.ViewModels;
using System.Net;
using System.Windows;
using System.Windows.Media;

namespace LANPaint.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            var context = new PaintViewModel(new BroadcastChainer(new UDPBroadcastImpl(IPAddress.Parse("192.168.0.100"))), new WPFDialogService());
            DataContext = context;

            context.Background = Color.FromRgb(255, 255, 255);
        }
    }
}
