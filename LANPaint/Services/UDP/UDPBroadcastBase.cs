using System;
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

        public UDPBroadcastBase() : this(9876)
        { }

        public UDPBroadcastBase(int port)
        {
            LocalIp = GetLocalIP();
            Port = port;
            Client = new UdpClient(new IPEndPoint(LocalIp, Port));
        }

        public UDPBroadcastBase(string ip, int port)
        {
            LocalIp = IPAddress.Parse(ip);
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

        protected virtual IPAddress GetLocalIP()
        {
            var ipHelper = new IPAddressHelper();
            var address = ipHelper.GetEthernetLocalIP();

            if (address.Equals(IPAddress.None))
            {
                address = ipHelper.GetWirelessLocalIP();

                if (address.Equals(IPAddress.None))
                {
                    throw new Exception("Local NIC with IPv4 address not found!");
                }
            }

            return address;
        }

        public void Dispose()
        {
            Client?.Dispose();
        }
    }
}
