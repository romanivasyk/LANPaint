using System.Net;

namespace LANPaint.Services.UDP.Factory
{
    public interface IUDPBroadcastFactory
    {
        IUDPBroadcast Create(IPAddress ipAddress);
        IUDPBroadcast Create(IPAddress ipAddress, int port);
    }
}
