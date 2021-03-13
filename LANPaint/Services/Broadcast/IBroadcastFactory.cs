using System.Net;

namespace LANPaint.Services.Broadcast
{
    public interface IBroadcastFactory
    {
        public IBroadcast Create(IPAddress ipAddress);
        public IBroadcast Create(IPAddress ipAddress, int port);
    }
}
