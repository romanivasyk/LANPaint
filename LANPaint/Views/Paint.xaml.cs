using System;
using LANPaint.Dialogs.Service;
using LANPaint.Services.UDP.Factory;
using LANPaint.ViewModels;
using System.Windows;
using System.Windows.Media;

namespace LANPaint.Views
{
    public partial class Paint : Window
    {
        private readonly ChainerFactory _factory;

        public Paint()
        {
            InitializeComponent();
            _factory = new ChainerFactory();
            var context = new PaintViewModel(_factory, new DialogService());
            DataContext = context;

            context.Background = Color.FromRgb(255, 255, 255);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is IDisposable disposableContext) disposableContext.Dispose();
        }
    }
}
