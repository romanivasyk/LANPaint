using System.Collections.Generic;
using System.Net.Sockets;

namespace LANPaint.Services.FirewallConfiguration.Model
{
    public struct FirewallEntry
    {
        public bool Enabled;
        public Direction Direction;
        public List<Profile> Profile;
        public string LocalIp;
        public string RemoteIp;
        public string LocalPort;
        public string RemotePort;
        public ProtocolType Protocol;
        public string Program;
        public Action Action;
    }
}