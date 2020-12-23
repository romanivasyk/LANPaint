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
        private IPAddress _localIp;

        public UDPBroadcastService()
        {
            _localIp = IPAddress.Parse("192.168.0.103");
            _client = new UdpClient(new IPEndPoint(_localIp, Port));
            _client.MulticastLoopback = false;
            _client.DontFragment = false;
        }

        public Task<int> SendAsync(byte[] bytes)
        {
            return Task.Run(() => _client.Send(bytes, bytes.Length, IPAddress.Broadcast.ToString(), Port));
        }

        public async Task<byte[]> ReceiveAsync()
        {
            var result = await _client.ReceiveAsync();
            if(result.RemoteEndPoint.Address.ToString() == _localIp.ToString())
            {
                return null;
            }

            return result.Buffer;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
