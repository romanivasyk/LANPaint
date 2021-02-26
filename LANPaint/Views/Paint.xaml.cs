using LANPaint.Dialogs.FrameworkDialogs;
using LANPaint.Dialogs.Service;
using LANPaint.Services.Broadcast.UDP.Factories;
using LANPaint.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using LANPaint.Services.Broadcast;
using LANPaint.Services.IO;
using LANPaint.Services.Network;

namespace LANPaint.Views
{
    public partial class Paint : Window
    {
        public Paint()
        {
            InitializeComponent();
            var broadcastFactory = new ChainerFactory(49152);
            var broadcastService = new BroadcastService(broadcastFactory, NetworkInterfaceHelper.GetInstance());
            var frameworkDialogFactory = new DefaultFrameworkDialogFactory();
            var dialogService = new DefaultDialogService(frameworkDialogFactory);
            var fileService = new DefaultFileService();

            var context = new PaintViewModel(broadcastService, dialogService, fileService);
            DataContext = context;

            context.Background = Color.FromRgb(255, 255, 255);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is IDisposable disposableContext) disposableContext.Dispose();
            NetworkInterfaceHelper.GetInstance().Dispose();
        }
    }
}
