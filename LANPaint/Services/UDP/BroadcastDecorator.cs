using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public abstract class BroadcastDecorator : IBroadcast
    {
        public IPEndPoint LocalEndPoint => Broadcast.LocalEndPoint;

        protected readonly IBroadcast Broadcast;

        protected BroadcastDecorator(IBroadcast broadcast)
        {
            Broadcast = broadcast;
        }

        public abstract Task<byte[]> ReceiveAsync(CancellationToken token = default);
        public abstract Task<int> SendAsync(byte[] bytes);
        public abstract ValueTask ClearBufferAsync();
        public abstract void Dispose();
    }
}
