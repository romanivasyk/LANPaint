using System;
using System.Collections.Immutable;
using System.Net.NetworkInformation;

namespace LANPaint.Services.Network.Watchers
{
    public interface INetworkWatcher : IDisposable
    {
        public bool IsAnyNetworkAvailable { get; }
        public event NetworkStateChangedEventHandler NetworkStateChanged;
        public ImmutableArray<NetworkInterface> Interfaces { get; }
    }

    public delegate void NetworkStateChangedEventHandler(object sender, EventArgs e);
}