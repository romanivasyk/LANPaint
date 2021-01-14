using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP
{
    public abstract class UDPBroadcastBase : IDisposable
    {
        public UdpClient Client { get; }
        public int Port { get; }
        public IPAddress LocalIp { get; }

        public UDPBroadcastBase()
        {
            var ipHelper = new IPAddressHelper();
            LocalIp = ipHelper.GetEthernetLocalIP();

            if(LocalIp.Equals(IPAddress.None))
            {
                LocalIp = ipHelper.GetWirelessLocalIP();

                if (LocalIp.Equals(IPAddress.None))
                {
                    throw new Exception("Local NIC with IPv4 address not found!");
                }
            } 

            Port = 9876;
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
