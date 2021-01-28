using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public abstract class UDPBroadcastBase : INetworkBroadcaster
    {
        public UdpClient Client { get; }
        public int Port { get; }
        public IPAddress LocalIp { get; }

        public UDPBroadcastBase(IPAddress iPAddress) : this(iPAddress, 9876) { }

        public UDPBroadcastBase(IPAddress iPAddress, int port)
        {
            LocalIp = iPAddress;
            Port = port;
            Client = new UdpClient(new IPEndPoint(LocalIp, Port));
        }

        public abstract Task<long> SendAsync(byte[] bytes);
        public abstract Task<byte[]> ReceiveAsync();

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
        }
    }
}
