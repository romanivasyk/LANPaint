using System.Net;
using System.Net.NetworkInformation;

namespace LANPaint.Services.UDP
{
    internal class IPAddressHelper
    {
        public IPAddress GetEthernetLocalIP()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var address = GetIPv4(nic, NetworkInterfaceType.Ethernet);
                if (!address.Equals(IPAddress.None))
                {
                    return address;
                }
            }

            return IPAddress.None;
        }

        public IPAddress GetWirelessLocalIP()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                var address = GetIPv4(nic, NetworkInterfaceType.Wireless80211);
                if (!address.Equals(IPAddress.None))
                {
                    return address;
                }
            }

            return IPAddress.None;
        }

        private static IPAddress GetIPv4(NetworkInterface nic, NetworkInterfaceType type)
        {
            if (nic.NetworkInterfaceType == type)
            {
                foreach (UnicastIPAddressInformation ipInfo in nic.GetIPProperties().UnicastAddresses)
                {
                    if (ipInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ipInfo.Address;
                    }
                }
            }

            return IPAddress.None;
        }
    }
}
