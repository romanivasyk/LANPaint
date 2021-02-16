using LANPaint.Services.Network;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LANPaint.Services.Broadcast
{
    public class BroadcastService : IBroadcastService
    {
        public event Action ConnectionLost;
        public event DataReceivedEventHandler DataReceived;

        public IPEndPoint LocalEndPoint =>
            _broadcastImpl == null ? new IPEndPoint(IPAddress.None, 0) : _broadcastImpl.LocalEndPoint;
        public bool IsReady { get; private set; }
        public bool IsReceiving { get; private set; }

        private bool _isDisposed;

        private IBroadcast _broadcastImpl;
        private CancellationTokenSource _cancelReceiveTokenSource;
        private readonly IBroadcastFactory _broadcastFactory;
        private readonly NetworkInterfaceHelper _networkInterfaceHelper;
        private readonly Dispatcher _dispatcher;

        public BroadcastService(IBroadcastFactory broadcastFactory)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _broadcastFactory = broadcastFactory ?? throw new ArgumentNullException(nameof(broadcastFactory));
            _networkInterfaceHelper = NetworkInterfaceHelper.GetInstance();
            _networkInterfaceHelper.Interfaces.CollectionChanged += NetworkInterfacesCollectionChanged;
        }

        private void NetworkInterfacesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_broadcastImpl != null && _networkInterfaceHelper.IsReadyToUse(_broadcastImpl.LocalEndPoint.Address)) return;

            _cancelReceiveTokenSource?.Dispose();
            IsReady = IsReceiving = false;
            _broadcastImpl?.Dispose();
            _broadcastImpl = null;

            //ObservableCollection uses worker thread to notify about change,
            //so we should use Main Thread for UI related code.
            _dispatcher.Invoke(() => ConnectionLost?.Invoke());
        }

        public bool Initialize(IPAddress ipAddress, int port = default)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(BroadcastService));
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
            if (IsReady && Equals(LocalEndPoint, new IPEndPoint(ipAddress, port))) return true;

            if (!_networkInterfaceHelper.IsReadyToUse(ipAddress)) return false; // Throw exception?

            _broadcastImpl?.Dispose();
            _broadcastImpl = port == default
                ? _broadcastFactory.Create(ipAddress)
                : _broadcastFactory.Create(ipAddress, port);

            IsReady = true;
            return true;
        }

        public async Task<int> SendAsync(byte[] data)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(BroadcastService));
            if (!IsReady)
                throw new ServiceNotInitializedException(
                    "Initialize() should be called to be able to use BroadcastService.");

            var bytesBroadcasted = 0;
            try
            {
                bytesBroadcasted = await _broadcastImpl.SendAsync(data);
            }
            catch (SocketException) when (!_networkInterfaceHelper.IsReadyToUse(LocalEndPoint.Address))
            { }

            return bytesBroadcasted;
        }

        public async Task StartReceive()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(BroadcastService));
            if (!IsReady) throw new ServiceNotInitializedException(
                "Initialize() should be called to be able to use BroadcastService.");

            if (IsReceiving) return; // Throw exception here?

            _cancelReceiveTokenSource?.Dispose();
            _cancelReceiveTokenSource = new CancellationTokenSource();

            IsReceiving = true;

            await _broadcastImpl.ClearBufferAsync();

            while (true)
            {
                byte[] data = default;
                try
                {
                    data = await _broadcastImpl.ReceiveAsync(_cancelReceiveTokenSource.Token);
                }
                catch (OperationCanceledException) when (_cancelReceiveTokenSource.IsCancellationRequested)
                { }
                catch (AggregateException exception) when (
                    exception.InnerException is ObjectDisposedException disposedException &&
                    (disposedException.ObjectName == typeof(Socket).FullName ||
                     disposedException.ObjectName == typeof(UdpClient).FullName ||
                     disposedException.ObjectName == typeof(TcpClient).FullName))
                { }
                catch (SocketException) when (!_networkInterfaceHelper.IsReadyToUse(LocalEndPoint.Address))
                {
                    IsReady = false;
                }
                finally
                {
                    _cancelReceiveTokenSource?.Dispose();
                    //_cancelReceiveTokenSource = null;
                    IsReceiving = false;
                }

                if (!IsReceiving) return;

                if (data != null && data.Length > 0)
                    DataReceived?.Invoke(this, new DataReceivedEventArgs(data));
            }
        }

        public void CancelReceive()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(BroadcastService));

            _cancelReceiveTokenSource?.Cancel();
            IsReceiving = false;
        }

        public void Dispose()
        {
            if (IsReceiving)
            {
                _cancelReceiveTokenSource.Cancel();
                IsReceiving = false;
            }

            _broadcastImpl?.Dispose();
            IsReady = false;
            _networkInterfaceHelper.Interfaces.CollectionChanged -= NetworkInterfacesCollectionChanged;
            _isDisposed = true;
        }
    }

    public class ServiceNotInitializedException : Exception
    {
        public ServiceNotInitializedException(string message = null, Exception innerException = null) : base(message, innerException)
        { }
    }
}
