using System;
using System.Net;

namespace LANPaint.Services.UDP.Factory
{
    public class ChainerFactory : IUDPBroadcastFactory,IDisposable
    {
        private IUDPBroadcast _cachedInstance;
        public IUDPBroadcast Create(IPAddress ipAddress)
        {
            _cachedInstance?.Dispose();

            _cachedInstance = new Chainer(new UDPBroadcastImpl(ipAddress));
            return _cachedInstance;
        }

        public IUDPBroadcast Create(IPAddress ipAddress, int port)
        {
            _cachedInstance?.Dispose();

            _cachedInstance = new Chainer(new UDPBroadcastImpl(ipAddress, port));
            return _cachedInstance;
        }

        public IUDPBroadcast Create(IPAddress ipAddress, int port, params object[] additionalParams)
        {
            var segmentLength = (int)additionalParams[0];

            if (segmentLength < 1024)
            {
                throw new ArgumentException("Segment length should be more than 1024 bytes.",
                    nameof(additionalParams));
            }
            _cachedInstance?.Dispose();

            _cachedInstance = new Chainer(new UDPBroadcastImpl(ipAddress, port), segmentLength);
            return _cachedInstance;
        }

        public void Dispose()
        {
            _cachedInstance?.Dispose();
        }
    }
}
