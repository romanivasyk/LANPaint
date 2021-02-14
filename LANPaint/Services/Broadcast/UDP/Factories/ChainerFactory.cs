using LANPaint.Services.Broadcast.UDP.Decorators;
using System;
using System.Net;

namespace LANPaint.Services.Broadcast.UDP.Factories
{
    public class ChainerFactory : IBroadcastFactory, IDisposable
    {
        private IBroadcast _cachedInstance;
        public int SegmentLength { get; }

        public ChainerFactory(int segmentLength = 8192)
        {
            if (segmentLength < 1024)
                throw new ArgumentException("Provided segment length should be more than 1023.", nameof(segmentLength));
            SegmentLength = segmentLength;
        }

        public IBroadcast Create(IPAddress ipAddress)
        {
            _cachedInstance?.Dispose();

            _cachedInstance = SegmentLength < 1024
                ? new Chainer(new UdpBroadcastImpl(ipAddress))
                : new Chainer(new UdpBroadcastImpl(ipAddress), SegmentLength);
            return _cachedInstance;
        }

        public IBroadcast Create(IPAddress ipAddress, int port)
        {
            _cachedInstance?.Dispose();

            _cachedInstance = SegmentLength < 1024
                ? new Chainer(new UdpBroadcastImpl(ipAddress, port))
                : new Chainer(new UdpBroadcastImpl(ipAddress, port), SegmentLength);
            return _cachedInstance;
        }

        public void Dispose()
        {
            _cachedInstance?.Dispose();
        }
    }
}
