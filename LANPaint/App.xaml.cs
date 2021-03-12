using System.Windows;
using LANPaint.Dialogs.FrameworkDialogs;
using LANPaint.Dialogs.Service;
using LANPaint.Services.Broadcast;
using LANPaint.Services.Broadcast.Factories;
using LANPaint.Services.IO;
using LANPaint.Services.Network;
using LANPaint.ViewModels;
using LANPaint.Views;

namespace LANPaint
{
    public partial class App : Application
    {
        private PaintViewModel _paintDataContext;

        private INetworkInterfaceHelper _networkInterfaceHelper;
        
        //TODO: Move this stuff to some kind of Setup or ApplicationBootstrapper with DI container.
        //https://www.codeproject.com/Articles/812379/Using-Ninject-to-produce-a-loosely-coupled-modular
        private void OnStartupHandler(object sender, StartupEventArgs e)
        {
            var broadcastFactory = new ChainerFactory(16384);
            _networkInterfaceHelper = NetworkInterfaceHelper.GetInstance();
            var broadcastService = new BroadcastService(broadcastFactory, _networkInterfaceHelper);
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
            _networkInterfaceHelper.Dispose();
        }
    }
}
