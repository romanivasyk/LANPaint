using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LANPaint.Services.Network.Utilities;
using LANPaint.Services.Network.Watchers;

namespace LANPaint.Services.Broadcast
{
    public class BroadcastService : IBroadcastService
    {
        public event EventHandler ConnectionLost;
        public event DataReceivedEventHandler DataReceived;

        public IPEndPoint LocalEndPoint =>
            _broadcastImpl == null ? new IPEndPoint(IPAddress.None, 0) : _broadcastImpl.LocalEndPoint;
        public bool IsReady { get; private set; }
        public bool IsReceiving { get; private set; }

        private bool _isDisposed;

        private IBroadcast _broadcastImpl;
        private CancellationTokenSource _cancelReceiveTokenSource;
        private readonly IBroadcastFactory _broadcastFactory;
        private readonly INetworkWatcher _networkWatcher;
        private readonly INetworkUtility _networkUtility;
        
        public BroadcastService(IBroadcastFactory broadcastFactory, INetworkWatcher networkWatcher, INetworkUtility networkUtility)
        {
            _broadcastFactory = broadcastFactory ?? throw new ArgumentNullException(nameof(broadcastFactory));
            _networkWatcher = networkWatcher ?? throw new ArgumentNullException(nameof(networkWatcher));
            _networkUtility = networkUtility ?? throw new ArgumentNullException(nameof(networkUtility));
            _networkWatcher.NetworkStateChanged += NetworkStateChangedHandler;
        }

        private void NetworkStateChangedHandler(object sender, EventArgs e)
        {
            if (!IsReady || _networkUtility.IsReadyToUse(_broadcastImpl.LocalEndPoint.Address)) return;
            
            _cancelReceiveTokenSource?.Dispose();
            IsReady = IsReceiving = false;
            _broadcastImpl?.Dispose();
            _broadcastImpl = null;
            
            ConnectionLost?.Invoke(this, EventArgs.Empty);
        }

        public bool Initialize(IPAddress ipAddress, int port = default)
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(BroadcastService));
            if (ipAddress == null) throw new ArgumentNullException(nameof(ipAddress));
            if (IsReady && Equals(LocalEndPoint, new IPEndPoint(ipAddress, port))) return true;

            if (!_networkUtility.IsReadyToUse(ipAddress)) return false; //Throw exception here?

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
            catch (SocketException) when (!_networkUtility.IsReadyToUse(LocalEndPoint.Address))
            { }

            return bytesBroadcasted;
        }

        public async Task StartReceiveAsync()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(BroadcastService));
            if (!IsReady)
                throw new ServiceNotInitializedException(
                    "Initialize() should be called to be able to use BroadcastService.");

            if (IsReceiving) return; //Throw exception here?

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
                {
                    _cancelReceiveTokenSource?.Dispose();
                    IsReceiving = false;
                }
                catch (AggregateException exception) when (
                    exception.InnerException is ObjectDisposedException disposedException &&
                    (disposedException.ObjectName == typeof(Socket).FullName ||
                     disposedException.ObjectName == typeof(UdpClient).FullName ||
                     disposedException.ObjectName == typeof(TcpClient).FullName))
                {
                    _cancelReceiveTokenSource?.Dispose();
                    IsReceiving = false;
                    
                    if (_broadcastImpl is null || Equals(new IPEndPoint(IPAddress.None, 0), _broadcastImpl.LocalEndPoint))
                        IsReady = false;
                }
                catch (SocketException) when (!_networkUtility.IsReadyToUse(LocalEndPoint.Address))
                {
                    _cancelReceiveTokenSource?.Dispose();
                    IsReceiving = IsReady = false;
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
            IsReady = false;
            _broadcastImpl?.Dispose();
            _broadcastImpl = null;
            _networkWatcher.NetworkStateChanged -= NetworkStateChangedHandler;
            _isDisposed = true;
        }
    }

    public class ServiceNotInitializedException : Exception
    {
        public ServiceNotInitializedException(string message = null, Exception innerException = null) : base(message,
            innerException)
        { }
    }
}