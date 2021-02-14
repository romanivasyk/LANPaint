using System.Net;

namespace LANPaint.Services.Broadcast
{
    public interface IBroadcastFactory
    {
        IBroadcast Create(IPAddress ipAddress);
        IBroadcast Create(IPAddress ipAddress, int port);
    }
}
