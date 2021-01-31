using System;
using System.Net;

namespace LANPaint.Services.UDP.Factory
{
    public class ChainerFactory : IUDPBroadcastFactory, IDisposable
    {
        private IUDPBroadcast _cachedInstance;
        public int SegmentLength { get; }

        public ChainerFactory(int segmentLength = default)
        {
            if (segmentLength > 0 && segmentLength < 1024)
                throw new ArgumentException("Provided segment length should be more than 1023.", nameof(segmentLength));
            SegmentLength = segmentLength;
        }

        public IUDPBroadcast Create(IPAddress ipAddress)
        {
            _cachedInstance?.Dispose();

            _cachedInstance = SegmentLength < 1024
                ? new Chainer(new UDPBroadcastImpl(ipAddress))
                : new Chainer(new UDPBroadcastImpl(ipAddress), SegmentLength);
            return _cachedInstance;
        }

        public IUDPBroadcast Create(IPAddress ipAddress, int port)
        {
            _cachedInstance?.Dispose();

            _cachedInstance = SegmentLength < 1024
                ? new Chainer(new UDPBroadcastImpl(ipAddress, port))
                : new Chainer(new UDPBroadcastImpl(ipAddress, port), SegmentLength);
            return _cachedInstance;
        }

        public void Dispose()
        {
            _cachedInstance?.Dispose();
        }
    }
}
