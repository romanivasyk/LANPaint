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
            var broadcastFactory = new ChainerFactory(16384);
            var broadcastService = new BroadcastService(broadcastFactory);
            var frameworkDialogFactory = new DefaultFrameworkDialogFactory();
            var dialogService = new DefaultDialogService(frameworkDialogFactory);
            var fileService = new DefaultFileService();

            var context = new PaintViewModel(broadcastService, dialogService, fileService);
            DataContext = context;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is IDisposable disposableContext) disposableContext.Dispose();
        }
    }
}