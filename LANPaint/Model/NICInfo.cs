using System.Net;

namespace LANPaint.Model
{
    public class NICInfo
    {
        public string Name { get; set; }
        public IPAddress IpAddress { get; set; }
        public bool IsReadyToUse { get; set; }
    }
}
