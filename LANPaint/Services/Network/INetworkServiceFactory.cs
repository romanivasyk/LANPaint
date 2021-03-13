using LANPaint.Services.Network.Utilities;
using LANPaint.Services.Network.Watchers;

namespace LANPaint.Services.Network
{
    public interface INetworkServiceFactory
    {
        INetworkWatcher CreateWatcher();
        INetworkUtility CreateUtility();
    }
}