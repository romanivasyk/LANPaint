using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public class UDPBroadcastImpl : UDPBroadcastBase
    {
        public UDPBroadcastImpl(IPAddress iPAddress) : base(iPAddress) { }
        public UDPBroadcastImpl(IPAddress iPAddress, int port) : base(iPAddress, port) { }

        public override Task<long> SendAsync(byte[] bytes)
        {
            return Task.Run(() => (long)Client.Send(bytes, bytes.Length, IPAddress.Broadcast.ToString(), Port));
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
