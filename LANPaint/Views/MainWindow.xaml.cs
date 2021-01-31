using LANPaint.Services.UDP;
using LANPaint.ViewModels;
using System.Net;
using System.Windows;
using System.Windows.Media;
using LANPaint.Dialog;

namespace LANPaint.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string iPAddress)
        {
            InitializeComponent();
            var context = new PaintViewModel(new Chainer(new UDPBroadcastImpl(IPAddress.Parse(iPAddress))), new WPFDialogService());
            DataContext = context;

            context.Background = Color.FromRgb(255, 255, 255);
        }
    }
}
