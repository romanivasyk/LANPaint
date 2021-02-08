using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public abstract class UDPBroadcastBase : IUDPBroadcast
    {
        public UdpClient Client { get; }
        public IPEndPoint LocalEndPoint { get; }
        public IPEndPoint BroadcastEndPoint { get; }

        protected UDPBroadcastBase(IPAddress iPAddress, int port = 9876)
        {
            LocalEndPoint = new IPEndPoint(iPAddress, port);
            Client = new UdpClient(LocalEndPoint);
            BroadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
        }

        public abstract Task<byte[]> ReceiveAsync(CancellationToken token = default);
        public abstract Task<int> SendAsync(byte[] bytes);

        public virtual async ValueTask ClearBufferAsync()
        {
            while (Client.Client.Available > 0)
            {
                await Client.ReceiveAsync();
            }
        }

        public void Dispose()
        {
            Client?.Dispose();

            LocalEndPoint.Address = IPAddress.None;
            LocalEndPoint.Port = 0;
        }
    }
}
