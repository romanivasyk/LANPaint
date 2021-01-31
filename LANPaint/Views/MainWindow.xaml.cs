using LANPaint.Dialog;
using LANPaint.Services.UDP.Factory;
using LANPaint.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace LANPaint.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ChainerFactory _factory;

        public MainWindow()
        {
            InitializeComponent();
            _factory = new ChainerFactory();
            var context = new PaintViewModel(_factory, new WPFDialogService());
            DataContext = context;

            context.Background = Color.FromRgb(255, 255, 255);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _factory?.Dispose();
        }
    }
}
