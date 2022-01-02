using LANPaint.Services.Network.Utilities;
using LANPaint.Services.Network.Watchers;

namespace LANPaint.Services.Network;

public class NetworkServiceFactory : INetworkServiceFactory
{
    public INetworkWatcher CreateWatcher() => new NetworkWatcher();
    public INetworkUtility CreateUtility() => new NetworkUtility();
}