using System.Net;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP
{
    public class UDPBroadcastImpl : UDPBroadcastBase
    {
        public UDPBroadcastImpl()
        { }

        public override Task<int> SendAsync(byte[] bytes)
        {
            return Task.Run(() => Client.Send(bytes, bytes.Length, IPAddress.Broadcast.ToString(), Port));
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
