using LANPaint.Dialogs.FrameworkDialogs;
using LANPaint.Dialogs.Service;
using LANPaint.Services.Broadcast.UDP.Factories;
using LANPaint.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using LANPaint.Services.Broadcast;
using LANPaint.Services.IO;

namespace LANPaint.Views
{
    public partial class Paint : Window
    {
        public Paint()
        {
            InitializeComponent();
            var broadcastFactory = new ChainerFactory();
            var context = new PaintViewModel(new BroadcastService(new ChainerFactory(49152)), new DefaultDialogService(new DefaultFrameworkDialogFactory()), new DefaultFileService());
            DataContext = context;

            context.Background = Color.FromRgb(255, 255, 255);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is IDisposable disposableContext) disposableContext.Dispose();
        }
    }
}
