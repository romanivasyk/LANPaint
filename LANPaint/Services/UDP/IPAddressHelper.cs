using System.Net;
using System.Net.NetworkInformation;

namespace LANPaint.Services.UDP
{
    internal class IPAddressHelper
    {
        public IPAddress GetEthernetLocalIP()
        {
            if (NetworkInterface.GetIsNetworkAvailable() == false)
            {
                return IPAddress.None;
            }

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && !nic.Name.Contains("Virtual"))
                {
                    var address = GetIPv4(nic, NetworkInterfaceType.Ethernet);
                    if (!address.Equals(IPAddress.None))
                    {
                        return address;
                    }
                }
            }

            return IPAddress.None;
        }

        public IPAddress GetWirelessLocalIP()
        {
            if (NetworkInterface.GetIsNetworkAvailable() == false)
            {
                return IPAddress.None;
            }

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up && !nic.Name.Contains("Virtual"))
                {
                    var address = GetIPv4(nic, NetworkInterfaceType.Wireless80211);
                    if (!address.Equals(IPAddress.None))
                    {
                        return address;
                    }
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
