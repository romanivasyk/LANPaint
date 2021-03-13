using System;
using System.Collections.Immutable;
using System.Net.NetworkInformation;

namespace LANPaint.Services.Network.Watchers
{
    public interface INetworkWatcher : IDisposable
    {
        bool IsAnyNetworkAvailable { get; }
        event NetworkStateChangedEventHandler NetworkStateChanged;
        ImmutableArray<NetworkInterface> Interfaces { get; }
    }

    public delegate void NetworkStateChangedEventHandler(object sender, EventArgs e);
}