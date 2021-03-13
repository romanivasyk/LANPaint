using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

#nullable enable
namespace LANPaint.Services.Network.Watchers
{
    public class NetworkWatcher : INetworkWatcher
    {
        public bool IsAnyNetworkAvailable { get; private set; }
        public ImmutableArray<NetworkInterface> Interfaces { get; private set; }
        public event NetworkStateChangedEventHandler? NetworkStateChanged;
        private readonly SynchronizationContext? _synchronizationContext;

        public NetworkWatcher()
        {
            IsAnyNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            Interfaces = GetIPv4Interfaces();
            _synchronizationContext = SynchronizationContext.Current;
            NetworkChange.NetworkAddressChanged += AddressChangedHandler;
            NetworkChange.NetworkAvailabilityChanged += AvailabilityChangedHandler;
        }

        private void AddressChangedHandler(object? sender, EventArgs e)
        {
            Interfaces = GetIPv4Interfaces();
            NotifyStateChanged();
        }

        private void AvailabilityChangedHandler(object? sender, NetworkAvailabilityEventArgs e)
        {
            IsAnyNetworkAvailable = e.IsAvailable;
            Interfaces = GetIPv4Interfaces();
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            if (_synchronizationContext is null)
            {
                NetworkStateChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            _synchronizationContext.Send(state => NetworkStateChanged?.Invoke(this, EventArgs.Empty), null);
        }

        private static ImmutableArray<NetworkInterface> GetIPv4Interfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces().Where(networkInterface =>
                (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                 networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211) &&
                !networkInterface.Name.Contains("Virtual") &&
                networkInterface.GetIPProperties().UnicastAddresses.Any(addressInformation =>
                    addressInformation.Address.AddressFamily == AddressFamily.InterNetwork)).ToImmutableArray();
        }

        public void Dispose()
        {
            NetworkChange.NetworkAddressChanged -= AddressChangedHandler;
            NetworkChange.NetworkAvailabilityChanged -= AvailabilityChangedHandler;
        }
    }
}