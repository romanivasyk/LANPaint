using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP.Test
{
    public abstract class UDPBroadcastBase : IDisposable
    {
        public UdpClient Client { get; }
        public int Port { get; }
        public IPAddress LocalIp { get; }

        public UDPBroadcastBase()
        {
            LocalIp = IPAddress.Parse("192.168.0.103");
            Port = 10000;
            Client = new UdpClient(new IPEndPoint(LocalIp, Port));
        }

        public UDPBroadcastBase(string ip, int port)
        {
            LocalIp = IPAddress.Parse(ip);
            Port = port;
            Client = new UdpClient(new IPEndPoint(LocalIp, Port));
        }

        public abstract Task<int> SendAsync(byte[] bytes);
        public abstract Task<byte[]> ReceiveAsync();
        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}
