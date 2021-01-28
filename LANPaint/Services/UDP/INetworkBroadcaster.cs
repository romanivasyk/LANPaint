using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public interface INetworkBroadcaster : IDisposable
    {
        Task<byte[]> ReceiveAsync();
        Task<long> SendAsync(byte[] bytes);
        ValueTask ClearBufferAsync();
    }
}