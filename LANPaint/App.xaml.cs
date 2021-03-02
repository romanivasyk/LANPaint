using System.Windows;
using LANPaint.Dialogs.FrameworkDialogs;
using LANPaint.Dialogs.Service;
using LANPaint.Services.Broadcast;
using LANPaint.Services.Broadcast.UDP.Factories;
using LANPaint.Services.IO;
using LANPaint.ViewModels;
using LANPaint.Views;

namespace LANPaint
{
    public partial class App : Application
    {
        private PaintViewModel _paintDataContext;
        
        //TODO: Move this stuff to some kind of Setup or ApplicationBootstrapper with DI container.
        private void OnStartupHandler(object sender, StartupEventArgs e)
        {
            var broadcastFactory = new ChainerFactory(16384);
            var broadcastService = new BroadcastService(broadcastFactory);
            var frameworkDialogFactory = new DefaultFrameworkDialogFactory();
            var dialogService = new DefaultDialogService(frameworkDialogFactory);
            var fileService = new DefaultFileService();

            _paintDataContext = new PaintViewModel(broadcastService, dialogService, fileService);

            var paint = new Paint {DataContext = _paintDataContext, };
            paint.Show();
        }

        private void OnExitHandler(object sender, ExitEventArgs e)
        {
            _paintDataContext.Dispose();
        }
    }
}
