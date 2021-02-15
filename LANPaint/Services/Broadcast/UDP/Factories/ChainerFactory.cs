using LANPaint.Services.Broadcast.UDP.Decorators;
using System;
using System.Net;

namespace LANPaint.Services.Broadcast.UDP.Factories
{
    public class ChainerFactory : IBroadcastFactory
    {
        private const int MinSegmentLength = 1024;
        public int SegmentLength { get; }

        public ChainerFactory(int segmentLength = 8192)
        {
            if (segmentLength < MinSegmentLength)
                throw new ArgumentException("Provided segment length should be more than 1023.", nameof(segmentLength));
            SegmentLength = segmentLength;
        }

        public IBroadcast Create(IPAddress ipAddress)
        {
            return SegmentLength < MinSegmentLength
                ? new Chainer(new UdpBroadcastImpl(ipAddress))
                : new Chainer(new UdpBroadcastImpl(ipAddress), SegmentLength);
        }

        public IBroadcast Create(IPAddress ipAddress, int port)
        {
            return SegmentLength < MinSegmentLength
                ? new Chainer(new UdpBroadcastImpl(ipAddress, port))
                : new Chainer(new UdpBroadcastImpl(ipAddress, port), SegmentLength);
        }
    }
}
