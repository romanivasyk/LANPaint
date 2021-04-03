using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LANPaint.Services.Network.Utilities
{
    public class NetworkUtility : INetworkUtility
    {
        public IPAddress GetIpAddress(NetworkInterface networkInterface)
        {
            return networkInterface.GetIPProperties().UnicastAddresses
                .FirstOrDefault(information => information.Address.AddressFamily == AddressFamily.InterNetwork)?.Address;
        }

        public bool IsReadyToUse(NetworkInterface networkInterface)
        {
            return networkInterface.OperationalStatus == OperationalStatus.Up;
        }

        public bool IsReadyToUse(IPAddress ipAddress)
        {
            return NetworkInterface.GetAllNetworkInterfaces().Any(networkInterface =>
                Equals(GetIpAddress(networkInterface), ipAddress) && IsReadyToUse(networkInterface));
        }
    }
}