using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public class UDPBroadcastImpl : UDPBroadcastBase
    {
        public UDPBroadcastImpl(IPAddress iPAddress) : base(iPAddress) { }
        public UDPBroadcastImpl(IPAddress iPAddress, int port) : base(iPAddress, port) { }

        public override Task<int> SendAsync(byte[] bytes)
        {
            return Client.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, Port));
        }

        public override async Task<byte[]> ReceiveAsync()
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
