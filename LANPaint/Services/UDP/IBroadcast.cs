using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public interface IBroadcast : IDisposable
    {
        IPEndPoint LocalEndPoint { get; }

        Task<byte[]> ReceiveAsync(CancellationToken token = default);
        Task<int> SendAsync(byte[] bytes);
        ValueTask ClearBufferAsync();
    }
}