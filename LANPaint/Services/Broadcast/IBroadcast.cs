using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.Broadcast
{
    public interface IBroadcast : IDisposable
    {
        public IPEndPoint LocalEndPoint { get; }

        public Task<byte[]> ReceiveAsync(CancellationToken token);
        public Task<int> SendAsync(byte[] bytes);
        public ValueTask ClearBufferAsync();
    }
}