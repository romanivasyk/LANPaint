using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP
{
    public class UDPBroadcastImpl : UDPBroadcastBase
    {
        public UDPBroadcastImpl() : base("192.168.0.103", 9876)
        {
            Client.MulticastLoopback = false;
            Client.DontFragment = false;
        }

        public override Task<int> SendAsync(byte[] bytes)
        {
            return Task.Run(() => Client.Send(bytes, bytes.Length, IPAddress.Broadcast.ToString(), Port));
        }

        public async override Task<byte[]> ReceiveAsync()
        {
            var result = await Client.ReceiveAsync();
            if (result.RemoteEndPoint.Address.ToString() == LocalIp.ToString())
            {
                return null;
            }

            return result.Buffer;
        }
    }
}
