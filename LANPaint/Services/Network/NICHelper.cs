using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LANPaint.Services.Network
{
    public static class NICHelper
    {
        public static IEnumerable<NetworkInterface> GetInterfaces() => NetworkInterface.GetAllNetworkInterfaces().Where(ni =>
            (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet || ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
            !ni.Name.Contains("Virtual") &&
            ni.GetIPProperties().UnicastAddresses.Any(addressInformation => addressInformation.Address.AddressFamily == AddressFamily.InterNetwork));

        public static bool IsReadyToUse(NetworkInterface netInterface) => netInterface.OperationalStatus == OperationalStatus.Up;
    }
}
