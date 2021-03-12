using LANPaint.Services.Broadcast.UDP.Decorators;
using System;
using System.Net;

namespace LANPaint.Services.Broadcast.UDP.Factories
{
    public class ChainerFactory : IBroadcastFactory
    {
        public int PayloadSegmentLength { get; }

        public ChainerFactory(int payloadSegmentLength)
        {
            if (payloadSegmentLength < Chainer.MinSegmentLength || payloadSegmentLength > Chainer.MaxSegmentLength)
                throw new ArgumentOutOfRangeException(nameof(payloadSegmentLength),
                    $"Provided segment length should be in range from {Chainer.MinSegmentLength} to {Chainer.MaxSegmentLength}");

            PayloadSegmentLength = payloadSegmentLength;
        }

        public IBroadcast Create(IPAddress ipAddress)
        {
            return new Chainer(new UdpBroadcastImpl(ipAddress), PayloadSegmentLength);
        }

        public IBroadcast Create(IPAddress ipAddress, int port)
        {
            return new Chainer(new UdpBroadcastImpl(ipAddress, port), PayloadSegmentLength);
        }
    }
}