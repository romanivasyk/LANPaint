using LANPaint_vNext.Model;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LANPaint_vNext.Services
{
    public class UDPBroadcastService:IDisposable
    {
        private UdpClient _client;
        private IPEndPoint _broadcastEndpoint;

        public UDPBroadcastService()
        {
            _broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, 36555);
        }

        public int Send(byte[] bytes)
        {
            return _client.Send(bytes, bytes.Length);
        }

        public byte[] Receive()
        {
            return _client.Receive(ref _broadcastEndpoint);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
