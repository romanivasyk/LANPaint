using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using LANPaint.Services.Broadcast;
using LANPaint.Services.Network.Utilities;
using LANPaint.Services.Network.Watchers;
using Xunit;
using Moq;

namespace LANPaint.UnitTests
{
    public class BroadcastServiceTests
    {
        private readonly Mock<IBroadcast> _broadcastImplMock;
        private readonly Mock<IBroadcastFactory> _broadcastFactoryMock;
        private readonly Mock<INetworkWatcher> _networkWatcherMock;
        private readonly Mock<INetworkUtility> _networkUtilityMock;
        private readonly IPAddress _ipAddress;
        
        public BroadcastServiceTests()
        {
            _broadcastImplMock = new Mock<IBroadcast>();
            _broadcastImplMock.Setup(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()))
                .ReturnsAsync((byte[] data) => data?.Length ?? 0);
            _broadcastFactoryMock = new Mock<IBroadcastFactory>();
            _broadcastFactoryMock.Setup(factory => factory.Create(It.IsAny<IPAddress>()))
                .Returns(() => _broadcastImplMock.Object);
            _broadcastFactoryMock.Setup(factory => factory.Create(It.IsAny<IPAddress>(), It.IsAny<int>()))
                .Returns(() => _broadcastImplMock.Object);

            _networkWatcherMock = new Mock<INetworkWatcher>();
            _networkUtilityMock = new Mock<INetworkUtility>();
            _ipAddress = IPAddress.Parse("192.168.0.100");
        }

        [Fact]
        public void CtorTest()
        {
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
        }

        [Fact]
        public void Ctor_ArgumentNullExceptionOnAnyNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new BroadcastService(null, _networkWatcherMock.Object, _networkUtilityMock.Object));
            Assert.Throws<ArgumentNullException>(() =>
                new BroadcastService(_broadcastFactoryMock.Object, null, _networkUtilityMock.Object));
            Assert.Throws<ArgumentNullException>(() =>
                new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object, null));
        }

        [Fact]
        public void Initialize_AfterDispose()
        {
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.Dispose();

            Assert.Throws<ObjectDisposedException>(() => broadcastService.Initialize(IPAddress.Any));
        }

        [Fact]
        public void Initialize_WithNullAddress()
        {
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            Assert.Throws<ArgumentNullException>(() => broadcastService.Initialize(null));
        }

        [Fact]
        public void Initialize_WithReadyToUseIpAddress()
        {
            var calledCreateWithAddress = false;
            var calledCreateWithAddressAndPort = false;

            _networkUtilityMock.Setup(utility =>
                    utility.IsReadyToUse(_ipAddress))
                .Returns((IPAddress ipAddress) => Equals(ipAddress, _ipAddress));

            _broadcastFactoryMock.Setup(factory => factory.Create(It.IsAny<IPAddress>()))
                .Returns(() => _broadcastImplMock.Object)
                .Callback(() => calledCreateWithAddress = true);
            _broadcastFactoryMock.Setup(factory => factory.Create(It.IsAny<IPAddress>(), It.IsAny<int>()))
                .Returns(() => _broadcastImplMock.Object)
                .Callback(() => calledCreateWithAddressAndPort = true);

            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            var result = broadcastService.Initialize(_ipAddress);

            Assert.True(result);
            Assert.True(broadcastService.IsReady);
            Assert.True(calledCreateWithAddress ^ calledCreateWithAddressAndPort);
            _broadcastFactoryMock.Verify(factory => factory.Create(_ipAddress), Times.AtMostOnce);
            _broadcastFactoryMock.Verify(factory => factory.Create(_ipAddress, 0), Times.AtMostOnce);
        }

        [Fact]
        public void Initialize_WithReadyToUseIpAddressTwice()
        {
            var calledCreateWithAddress = false;
            var calledCreateWithAddressAndPort = false;

            _broadcastImplMock.Setup(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(_ipAddress, 0));
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            _broadcastFactoryMock.Setup(factory => factory.Create(_ipAddress))
                .Returns(() => _broadcastImplMock.Object)
                .Callback(() => calledCreateWithAddress = true);
            _broadcastFactoryMock.Setup(factory => factory.Create(_ipAddress, It.IsAny<int>()))
                .Returns(() => _broadcastImplMock.Object)
                .Callback(() => calledCreateWithAddressAndPort = true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            var firstInitResult = broadcastService.Initialize(_ipAddress);
            var secondInitResult = broadcastService.Initialize(_ipAddress);

            Assert.True(firstInitResult);
            Assert.True(secondInitResult);
            Assert.True(broadcastService.IsReady);
            Assert.True(calledCreateWithAddress ^ calledCreateWithAddressAndPort);
            _broadcastFactoryMock.Verify(factory => factory.Create(_ipAddress), Times.AtMostOnce);
            _broadcastFactoryMock.Verify(factory => factory.Create(_ipAddress, 0), Times.AtMostOnce);
        }

        [Fact]
        public void Initialize_WithNotReadyToUseIpAddress()
        {
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(It.IsAny<IPAddress>())).Returns(false);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            var result = broadcastService.Initialize(_ipAddress);

            Assert.False(result);
            Assert.False(broadcastService.IsReady);
            _broadcastFactoryMock.Verify(factory => factory.Create(_ipAddress), Times.Never);
            _broadcastFactoryMock.Verify(factory => factory.Create(_ipAddress, It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Initialize_WithReadyToUseIpAddressAndPort()
        {
            const int port = 50100;

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            var result = broadcastService.Initialize(_ipAddress, port);

            Assert.True(result);
            Assert.True(broadcastService.IsReady);
            _broadcastFactoryMock.Verify(factory => factory.Create(It.IsAny<IPAddress>()), Times.Never);
            _broadcastFactoryMock.Verify(factory => factory.Create(_ipAddress, port), Times.Once);
        }

        [Fact]
        public void ConnectionLost_RaiseOnDisconnect()
        {
            var isConnectionLostInvoked = false;

            _networkUtilityMock.SetupSequence(utility => utility.IsReadyToUse(_ipAddress)).Returns(true)
                .Returns(false);
            _broadcastImplMock.SetupGet(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(_ipAddress, 0));
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.ConnectionLost += (sender, e) => isConnectionLostInvoked = true;
            broadcastService.Initialize(_ipAddress);

            _networkWatcherMock.Raise(watcher => watcher.NetworkStateChanged -= null, EventArgs.Empty);

            Assert.True(isConnectionLostInvoked);
            Assert.False(broadcastService.IsReady);
        }

        [Fact]
        public void ConnectionLost_DontRaiseOnDisconnectNotUsedAdapter()
        {
            var isConnectionLostInvoked = false;

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            _broadcastImplMock.SetupGet(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(_ipAddress, 0));
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.ConnectionLost += (sender, e) => isConnectionLostInvoked = true;
            broadcastService.Initialize(_ipAddress);

            _networkWatcherMock.Raise(watcher => watcher.NetworkStateChanged -= null, EventArgs.Empty);

            Assert.False(isConnectionLostInvoked);
            Assert.True(broadcastService.IsReady);
        }

        [Fact]
        public void ConnectionLost_DontRaiseOnNotInitialized()
        {
            var isConnectionLostInvoked = false;

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            _broadcastImplMock.SetupGet(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(_ipAddress, 0));
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.ConnectionLost += (sender, e) => isConnectionLostInvoked = true;

            _networkWatcherMock.Raise(watcher => watcher.NetworkStateChanged -= null, EventArgs.Empty);

            Assert.False(isConnectionLostInvoked);
            Assert.False(broadcastService.IsReady);
        }

        [Fact]
        public void ConnectionLost_DontRaiseOnDisposed()
        {
            var isConnectionLostInvoked = false;

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            _broadcastImplMock.SetupGet(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(_ipAddress, 0));
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.ConnectionLost += (sender, e) => isConnectionLostInvoked = true;
            broadcastService.Initialize(_ipAddress);
            broadcastService.Dispose();

            _networkWatcherMock.Raise(watcher => watcher.NetworkStateChanged -= null, EventArgs.Empty);

            Assert.False(isConnectionLostInvoked);
            Assert.False(broadcastService.IsReady);
        }

        [Fact]
        public async Task SendAsync_SendData()
        {
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.Initialize(_ipAddress);
            var data = RandomizeByteSequence(1024);

            var bytesSent = await broadcastService.SendAsync(data);

            Assert.Equal(data.Length, bytesSent);
            _broadcastImplMock.Verify(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()));
        }

        [Fact]
        public async Task SendAsync_ThrowOnDisposed()
        {
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.Initialize(_ipAddress);
            var data = RandomizeByteSequence(1024);

            broadcastService.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => broadcastService.SendAsync(data));
        }

        [Fact]
        public async Task SendAsync_ThrowOnUninitialized()
        {
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            var data = RandomizeByteSequence(1024);

            await Assert.ThrowsAsync<ServiceNotInitializedException>(() => broadcastService.SendAsync(data));
        }

        [Fact]
        public async Task ReceiveAsync_ThrowOnDisposed()
        {
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.Initialize(_ipAddress);
            var data = RandomizeByteSequence(1024);

            broadcastService.Dispose();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => broadcastService.StartReceiveAsync());
            Assert.False(broadcastService.IsReceiving);
        }

        [Fact]
        public async Task ReceiveAsync_ThrowOnNotInitialized()
        {
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            var data = RandomizeByteSequence(1024);

            await Assert.ThrowsAsync<ServiceNotInitializedException>(() => broadcastService.StartReceiveAsync());
            Assert.False(broadcastService.IsReceiving);
        }

        [Fact]
        public async Task ReceiveAsync_ReceiveData()
        {
            var bytesToReceive = RandomizeByteSequence(1024);
            var receivedData = new List<byte[]>();

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(_ipAddress)).Returns(true);
            _broadcastImplMock.SetupSequence(broadcast => broadcast.ReceiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(bytesToReceive).ReturnsAsync(() =>
                {
                    Task.Delay(5000).Wait();
                    return Array.Empty<byte>();
                });
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.DataReceived += (sender, args) => receivedData.Add(args.Data);
            broadcastService.Initialize(_ipAddress);
            var stopAfterHundredMillis = Task.Run(() =>
            {
                Task.Delay(100).Wait();
                broadcastService.CancelReceive();
            });
            var receiving = broadcastService.StartReceiveAsync();

            await Task.WhenAll(stopAfterHundredMillis, receiving);

            Assert.Single(receivedData);
            Assert.True(bytesToReceive.SequenceEqual(receivedData.First()));
        }

        private static byte[] RandomizeByteSequence(int length)
        {
            var random = new Random();
            var sequence = new byte[length];

            for (var i = 0; i < length; i++)
            {
                sequence[i] = (byte) random.Next(256);
            }

            return sequence;
        }
    }
}