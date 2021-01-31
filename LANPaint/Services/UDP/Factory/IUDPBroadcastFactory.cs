using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LANPaint.Services.UDP.Factory
{
    public interface IUDPBroadcastFactory
    {
        IUDPBroadcast Create(IPAddress ipAddress);
        IUDPBroadcast Create(IPAddress ipAddress, int port);
    }
}
