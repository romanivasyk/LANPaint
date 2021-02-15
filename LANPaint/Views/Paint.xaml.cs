using LANPaint.Dialogs.FrameworkDialogs;
using LANPaint.Dialogs.Service;
using LANPaint.Services.Broadcast.UDP.Factories;
using LANPaint.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using LANPaint.Services.IO;

namespace LANPaint.Views
{
    public partial class Paint : Window
    {
        private readonly ChainerFactory _broadcastFactory;

        public Paint()
        {
            InitializeComponent();
            _broadcastFactory = new ChainerFactory();
            var context = new PaintViewModel(_broadcastFactory, new DefaultDialogService(new DefaultFrameworkDialogFactory()), new DefaultFileService());
            DataContext = context;

            context.Background = Color.FromRgb(255, 255, 255);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is IDisposable disposableContext) disposableContext.Dispose();
        }
    }
}
