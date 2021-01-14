using System.Net;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP.Test
{
    public class UDPBroadcastImpl : UDPBroadcastBase
    {
        public UDPBroadcastImpl() { }
        public UDPBroadcastImpl(int port, string ipAddress) : base(ipAddress, port) { }

        public override Task<int> SendAsync(byte[] bytes)
        {
            return Task.Run(() => Client.Send(bytes, bytes.Length, IPAddress.Broadcast.ToString(), 9876));
        }

        public async override Task<byte[]> ReceiveAsync()
        {
            var result = await Client.ReceiveAsync().ConfigureAwait(false);
            if (result.RemoteEndPoint.Address.Equals(LocalIp))
            {
                return null;
            }

            return result.Buffer;
        }
    }
}
