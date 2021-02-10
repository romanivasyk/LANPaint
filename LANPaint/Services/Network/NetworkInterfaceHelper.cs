using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LANPaint.Services.Network
{
    public class NetworkInterfaceHelper : IDisposable
    {
        public bool IsAnyNetworkAvailable { get; private set; }

        public ObservableCollection<NetworkInterface> Interfaces { get; }

        private static NetworkInterfaceHelper _helper;
        private static readonly object Locker = new object();

        private NetworkInterfaceHelper()
        {
            Interfaces = new ObservableCollection<NetworkInterface>(GetIPv4Interfaces());
            IsAnyNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            NetworkChange.NetworkAvailabilityChanged += NetworkAvailabilityChangedHandler;
            NetworkChange.NetworkAddressChanged += NetworkAddressChangedHandler;
        }

        public static NetworkInterfaceHelper GetInstance()
        {
            if (_helper != null) return _helper;
            lock (Locker)
            {
                _helper ??= new NetworkInterfaceHelper();
            }

            return _helper;
        }

        private void NetworkAvailabilityChangedHandler(object sender, NetworkAvailabilityEventArgs e)
        {
            IsAnyNetworkAvailable = e.IsAvailable;
            UpdateInterfaceCollection();
        }

        private void NetworkAddressChangedHandler(object sender, System.EventArgs e)
        {
            UpdateInterfaceCollection();
        }

        private void UpdateInterfaceCollection()
        {
            Interfaces.Clear();
            GetIPv4Interfaces().ToList().ForEach(ni => Interfaces.Add(ni));
        }

        private IEnumerable<NetworkInterface> GetIPv4Interfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(ni =>
                (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                 ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
                !ni.Name.Contains("Virtual") &&
                ni.GetIPProperties().UnicastAddresses.Any(addressInformation =>
                    addressInformation.Address.AddressFamily == AddressFamily.InterNetwork));
        }

        public IPAddress GetAnyReadyToUseIPv4Address() => GetIpAddress(Interfaces.FirstOrDefault(IsReadyToUse));

        public IPAddress GetIpAddress(NetworkInterface ni) => ni?.GetIPProperties().UnicastAddresses
            .First(information => information.Address.AddressFamily == AddressFamily.InterNetwork).Address;

        public bool IsReadyToUse(NetworkInterface ni) => ni.OperationalStatus == OperationalStatus.Up;

        public void Dispose()
        {
            if (_helper == null) return;
            NetworkChange.NetworkAvailabilityChanged -= _helper.NetworkAvailabilityChangedHandler;
            NetworkChange.NetworkAddressChanged -= _helper.NetworkAddressChangedHandler;
            _helper = null;
        }
    }
}
