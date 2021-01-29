using System;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public interface INetworkBroadcaster : IDisposable
    {
        Task<byte[]> ReceiveAsync(CancellationToken token = default);
        Task<int> SendAsync(byte[] bytes);
        ValueTask ClearBufferAsync();
    }
}