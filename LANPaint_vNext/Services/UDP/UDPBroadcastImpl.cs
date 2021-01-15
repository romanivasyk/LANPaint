using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP
{
    public class UDPBroadcastImpl : UDPBroadcastBase
    {
        public UDPBroadcastImpl() { }
        public UDPBroadcastImpl(int port) : base(port) { }
        public UDPBroadcastImpl(int port, string ipAddress) : base(ipAddress, port) { }

        public override Task<int> SendAsync(byte[] bytes)
        {
            return Task.Run(() => Client.Send(bytes, bytes.Length, IPAddress.Broadcast.ToString(), Port));
        }

        public async override Task<byte[]> ReceiveAsync()
        {
            UdpReceiveResult result;
            do
            {
                result = await Client.ReceiveAsync().ConfigureAwait(false);
            } while (result.RemoteEndPoint.Address.Equals(LocalIp));

            return result.Buffer;
        }
    }
}
