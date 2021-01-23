using LANPaint.DialogServices;
using LANPaint.Services.UDP;
using LANPaint.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace LANPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            var context = new PaintViewModel(new BroadcastChainer(), new WPFDialogService());
            DataContext = context;

            context.Background = Color.FromRgb(255, 255, 255);
        }
    }
}
