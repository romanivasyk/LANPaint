using LANPaint.Services.Network;
using LANPaint.Services.Network.Utilities;
using LANPaint.Services.Network.Watchers;
using Xunit;

namespace LANPaint.UnitTests.Services.Network
{
    public class NetworkServiceFactoryTests
    {
        private readonly NetworkServiceFactory factory = new NetworkServiceFactory();
        
        [Fact]
        public void GetNetworkWatcher()
        {
            Assert.Equal(factory.CreateWatcher().GetType(), typeof(NetworkWatcher));
        }

        [Fact]
        public void GetNetworkUtility()
        {
            Assert.Equal(factory.CreateUtility().GetType(), typeof(NetworkUtility));
        }
    }
}