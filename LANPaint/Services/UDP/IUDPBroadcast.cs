using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public interface IUDPBroadcast : IDisposable
    {
        UdpClient Client { get; }
        IPEndPoint LocalEndPoint { get; }

        Task<byte[]> ReceiveAsync(CancellationToken token = default);
        Task<int> SendAsync(byte[] bytes);
        ValueTask ClearBufferAsync();
    }
}