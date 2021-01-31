using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public abstract class UDPBroadcastDecorator : IUDPBroadcast
    {
        public UdpClient Client => UdpBroadcast.Client;
        public IPEndPoint LocalEndPoint => UdpBroadcast.LocalEndPoint;

        protected readonly IUDPBroadcast UdpBroadcast;

        protected UDPBroadcastDecorator(IUDPBroadcast udpBroadcast)
        {
            UdpBroadcast = udpBroadcast;
        }

        public abstract Task<byte[]> ReceiveAsync(CancellationToken token = default);
        public abstract Task<int> SendAsync(byte[] bytes);
        public abstract ValueTask ClearBufferAsync();
        public abstract void Dispose();
    }
}
