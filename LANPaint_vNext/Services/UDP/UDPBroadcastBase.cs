using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP
{
    public abstract class UDPBroadcastBase : IDisposable
    {
        public UdpClient Client { get; }
        public int Port { get; } = 9876;
        public IPAddress LocalIp { get; }

        public UDPBroadcastBase() : this("192.168.0.103", 9876)
        { }

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
