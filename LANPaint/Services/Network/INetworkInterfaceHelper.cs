using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;

namespace LANPaint.Services.Network
{
    public interface INetworkInterfaceHelper : IDisposable
    {
        bool IsAnyNetworkAvailable { get; }
        ObservableCollection<NetworkInterface> Interfaces { get; }
        IPAddress GetAnyReadyToUseIPv4Address();
        IPAddress GetIpAddress(NetworkInterface ni);
        bool IsReadyToUse(NetworkInterface ni);
        bool IsReadyToUse(IPAddress ipAddress);
    }
}