using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services
{
    public class UDPBroadcastService : IDisposable
    {
        private UdpClient _client;
        private const int Port = 9876;

        public UDPBroadcastService()
        {
            _client = new UdpClient(new IPEndPoint(IPAddress.Any, Port));
        }

        public Task<int> SendAsync(byte[] bytes)
        {
            return Task.Run(() => _client.Send(bytes, bytes.Length, IPAddress.Broadcast.ToString(), Port));
        }

        public async Task<byte[]> ReceiveAsync()
        {
            var result = await _client.ReceiveAsync();
            return result.Buffer;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
