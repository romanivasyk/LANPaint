using System.Net;

namespace LANPaint.Services.UDP.Factory
{
    public interface IBroadcastFactory
    {
        IBroadcast Create(IPAddress ipAddress);
        IBroadcast Create(IPAddress ipAddress, int port);
    }
}
