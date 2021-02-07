using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LANPaint.Services.Network
{
    public class NetworkInterfaceHelper
    {
        public bool IsAnyNetworkAvailable { get; private set; }

        public NetworkInterfaceHelper()
        {
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChangedHandler;
        }

        private void NetworkAvailabilityChangedHandler(object sender, NetworkAvailabilityEventArgs e)
        {
            IsAnyNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
        }

        public IEnumerable<NetworkInterface> GetIPv4Interfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(ni =>
                (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                 ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
                 !ni.Name.Contains("Virtual") &&
                 ni.GetIPProperties().UnicastAddresses.Any(addressInformation =>
                 addressInformation.Address.AddressFamily == AddressFamily.InterNetwork));
        }

        public bool IsReadyToUse(NetworkInterface ni) => ni.OperationalStatus == OperationalStatus.Up;

        public IPAddress GetIpAddress(NetworkInterface ni) => ni?.GetIPProperties().UnicastAddresses
            .First(information => information.Address.AddressFamily == AddressFamily.InterNetwork).Address;
    }
}
