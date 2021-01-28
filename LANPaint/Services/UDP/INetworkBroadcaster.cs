using System;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public interface INetworkBroadcaster : IDisposable
    {
        Task<byte[]> ReceiveAsync();
        Task<int> SendAsync(byte[] bytes);
        ValueTask ClearBufferAsync();
    }
}