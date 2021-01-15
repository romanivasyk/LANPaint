using System;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP
{
    public interface INetworkBroadcaster : IDisposable
    {
        Task<byte[]> ReceiveAsync();
        Task<long> SendAsync(byte[] bytes);
        ValueTask ClearBufferAsync();
    }
}