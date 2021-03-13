using System.Linq;
using System.Windows;
using LANPaint.Dialogs.FrameworkDialogs;
using LANPaint.Dialogs.Service;
using LANPaint.Services.Broadcast;
using LANPaint.Services.Broadcast.Factories;
using LANPaint.Services.IO;
using LANPaint.Services.Network;
using LANPaint.Services.Network.Watchers;
using LANPaint.ViewModels;
using LANPaint.Views;

namespace LANPaint
{
    public partial class App : Application
    {
        private INetworkWatcher _watcher;
        private IBroadcastService _broadcastService;
        private PaintViewModel _paintDataContext;

        //TODO: Move this stuff to some kind of Setup or ApplicationBootstrapper with DI container.
        //https://www.codeproject.com/Articles/812379/Using-Ninject-to-produce-a-loosely-coupled-modular
        private void OnStartupHandler(object sender, StartupEventArgs e)
        {
            var broadcastFactory = new ChainerFactory(16384);
            var networkServiceFactory = new NetworkServiceFactory();
            _watcher = networkServiceFactory.CreateWatcher();
            _broadcastService = new BroadcastService(broadcastFactory, _watcher, networkServiceFactory.CreateUtility());
            InitializeBroadcastService(networkServiceFactory, _broadcastService);
            var frameworkDialogFactory = new DefaultFrameworkDialogFactory();
            var dialogService = new DefaultDialogService(frameworkDialogFactory);
            var fileService = new DefaultFileService();

            _paintDataContext = new PaintViewModel(_broadcastService, dialogService, fileService, networkServiceFactory);

            var paint = new Paint {DataContext = _paintDataContext,};
            paint.Show();
        }

        private void InitializeBroadcastService(INetworkServiceFactory networkServiceFactory, IBroadcastService broadcastService)
        {
            if (!_watcher.IsAnyNetworkAvailable) return;
            var networkUtility = networkServiceFactory.CreateUtility();
            var readyToUseInterface =
                _watcher.Interfaces.FirstOrDefault(networkInterface => networkUtility.IsReadyToUse(networkInterface));
            if (readyToUseInterface is not null) broadcastService.Initialize(networkUtility.GetIpAddress(readyToUseInterface));
        }

        private void OnExitHandler(object sender, ExitEventArgs e)
        {
            _paintDataContext.Dispose();
            _broadcastService.Dispose();
            _watcher.Dispose();
        }
    }
}