using System;
using System.Net;
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
            var validIpAddress = IPAddress.Parse("192.168.0.100");
            var calledCreateWithAddress = false;
            var calledCreateWithAddressAndPort = false;

            _networkUtilityMock.Setup(utility =>
                    utility.IsReadyToUse(It.IsAny<IPAddress>()))
                .Returns((IPAddress ipAddress) => Equals(ipAddress, validIpAddress));

            _broadcastFactoryMock.Setup(factory => factory.Create(It.IsAny<IPAddress>()))
                .Returns(() => _broadcastImplMock.Object)
                .Callback(() => calledCreateWithAddress = true);
            _broadcastFactoryMock.Setup(factory => factory.Create(It.IsAny<IPAddress>(), It.IsAny<int>()))
                .Returns(() => _broadcastImplMock.Object)
                .Callback(() => calledCreateWithAddressAndPort = true);

            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            var result = broadcastService.Initialize(validIpAddress);

            Assert.True(result);
            Assert.True(broadcastService.IsReady);
            Assert.True(calledCreateWithAddress ^ calledCreateWithAddressAndPort);
            _broadcastFactoryMock.Verify(factory => factory.Create(validIpAddress), Times.AtMostOnce);
            _broadcastFactoryMock.Verify(factory => factory.Create(validIpAddress, 0), Times.AtMostOnce);
        }

        [Fact]
        public void Initialize_WithReadyToUseIpAddressTwice()
        {
            var ipAddress = IPAddress.Parse("192.168.0.100");
            var calledCreateWithAddress = false;
            var calledCreateWithAddressAndPort = false;

            _broadcastImplMock.Setup(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(ipAddress, 0));
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(It.IsAny<IPAddress>())).Returns(true);
            _broadcastFactoryMock.Setup(factory => factory.Create(It.IsAny<IPAddress>()))
                .Returns(() => _broadcastImplMock.Object)
                .Callback(() => calledCreateWithAddress = true);
            _broadcastFactoryMock.Setup(factory => factory.Create(It.IsAny<IPAddress>(), It.IsAny<int>()))
                .Returns(() => _broadcastImplMock.Object)
                .Callback(() => calledCreateWithAddressAndPort = true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            var firstInitResult = broadcastService.Initialize(ipAddress);
            var secondInitResult = broadcastService.Initialize(ipAddress);

            Assert.True(firstInitResult);
            Assert.True(secondInitResult);
            Assert.True(broadcastService.IsReady);
            Assert.True(calledCreateWithAddress ^ calledCreateWithAddressAndPort);
            _broadcastFactoryMock.Verify(factory => factory.Create(ipAddress), Times.AtMostOnce);
            _broadcastFactoryMock.Verify(factory => factory.Create(ipAddress, 0), Times.AtMostOnce);
        }

        [Fact]
        public void Initialize_WithNotReadyToUseIpAddress()
        {
            var ipAddress = IPAddress.Parse("192.168.0.100");
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(It.IsAny<IPAddress>())).Returns(false);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            var result = broadcastService.Initialize(ipAddress);

            Assert.False(result);
            Assert.False(broadcastService.IsReady);
            _broadcastFactoryMock.Verify(factory => factory.Create(It.IsAny<IPAddress>()), Times.Never);
            _broadcastFactoryMock.Verify(factory => factory.Create(It.IsAny<IPAddress>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Initialize_WithReadyToUseIpAddressAndPort()
        {
            var ipAddress = IPAddress.Parse("192.168.0.100");
            const int port = 50100;

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(It.IsAny<IPAddress>())).Returns(true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);

            var result = broadcastService.Initialize(ipAddress, port);

            Assert.True(result);
            Assert.True(broadcastService.IsReady);
            _broadcastFactoryMock.Verify(factory => factory.Create(It.IsAny<IPAddress>()), Times.Never);
            _broadcastFactoryMock.Verify(factory => factory.Create(ipAddress, port), Times.Once);
        }

        [Fact]
        public void ConnectionLost_RaiseOnDisconnect()
        {
            var ipAddress = IPAddress.Parse("192.168.0.100");
            var isConnectionLostInvoked = false;

            _networkUtilityMock.SetupSequence(utility => utility.IsReadyToUse(ipAddress)).Returns(true)
                .Returns(false);
            _broadcastImplMock.SetupGet(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(ipAddress, 0));
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.ConnectionLost += (sender, e) => isConnectionLostInvoked = true;
            broadcastService.Initialize(ipAddress);

            _networkWatcherMock.Raise(watcher => watcher.NetworkStateChanged -= null, EventArgs.Empty);

            Assert.True(isConnectionLostInvoked);
            Assert.False(broadcastService.IsReady);
        }

        [Fact]
        public void ConnectionLost_DontRaiseOnDisconnectNotUsedAdapter()
        {
            var ipAddress = IPAddress.Parse("192.168.0.100");
            var isConnectionLostInvoked = false;

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(ipAddress)).Returns(true);
            _broadcastImplMock.SetupGet(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(ipAddress, 0));
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.ConnectionLost += (sender, e) => isConnectionLostInvoked = true;
            broadcastService.Initialize(ipAddress);

            _networkWatcherMock.Raise(watcher => watcher.NetworkStateChanged -= null, EventArgs.Empty);

            Assert.False(isConnectionLostInvoked);
            Assert.True(broadcastService.IsReady);
        }

        [Fact]
        public void ConnectionLost_DontRaiseOnNotInitialized()
        {
            var ipAddress = IPAddress.Parse("192.168.0.100");
            var isConnectionLostInvoked = false;

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(ipAddress)).Returns(true);
            _broadcastImplMock.SetupGet(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(ipAddress, 0));
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
            var ipAddress = IPAddress.Parse("192.168.0.100");
            var isConnectionLostInvoked = false;

            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(ipAddress)).Returns(true);
            _broadcastImplMock.SetupGet(broadcast => broadcast.LocalEndPoint).Returns(new IPEndPoint(ipAddress, 0));
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.ConnectionLost += (sender, e) => isConnectionLostInvoked = true;
            broadcastService.Initialize(ipAddress);
            broadcastService.Dispose();

            _networkWatcherMock.Raise(watcher => watcher.NetworkStateChanged -= null, EventArgs.Empty);

            Assert.False(isConnectionLostInvoked);
            Assert.False(broadcastService.IsReady);
        }

        [Fact]
        public async Task SendAsync_SendData()
        {
            var ipAddress = IPAddress.Parse("192.168.0.100");
            _networkUtilityMock.Setup(utility => utility.IsReadyToUse(ipAddress)).Returns(true);
            var broadcastService = new BroadcastService(_broadcastFactoryMock.Object, _networkWatcherMock.Object,
                _networkUtilityMock.Object);
            broadcastService.Initialize(ipAddress);
            var data = RandomizeByteSequence(1024);

            var bytesSent = await broadcastService.SendAsync(data);

            Assert.Equal(data.Length, bytesSent);
            _broadcastImplMock.Verify(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()));
        }

//TODO: Continue testing SendAsync method
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