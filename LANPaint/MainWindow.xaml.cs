using LANPaint.Services;
using LANPaint.ViewModels;
using System.Windows.Media;
using System.Windows;
using LANPaint.Services.UDP;

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
