using System.Net;

namespace LANPaint.Model
{
    public class UDPSettings
    {
        public IPAddress IpAddress { get; }
        public int Port { get; }

        public UDPSettings(IPAddress ipAddress, int port = default)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }
}
