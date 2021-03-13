using System.Net;
using System.Net.NetworkInformation;

namespace LANPaint.Services.Network.Utilities
{
    public interface INetworkUtility
    {
        public IPAddress GetIpAddress(NetworkInterface ni);
        public bool IsReadyToUse(NetworkInterface networkInterface);
        public bool IsReadyToUse(IPAddress ipAddress);
    }
}