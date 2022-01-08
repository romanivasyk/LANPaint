using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.Broadcast.UDP;

public class UdpBroadcastImpl : UdpBroadcastBase
{
    public UdpBroadcastImpl(IPAddress iPAddress) : base(iPAddress) { }
    public UdpBroadcastImpl(IPAddress iPAddress, int port) : base(iPAddress, port) { }

    public override Task<int> SendAsync(byte[] bytes) => Client.SendAsync(bytes, bytes.Length, BroadcastEndPoint);

    public override async Task<byte[]> ReceiveAsync(CancellationToken token = default)
    {
        UdpReceiveResult result;
        do
        {
            result = await Client.ReceiveAsync(token);
        } while (Equals(result.RemoteEndPoint, LocalEndPoint));

        return result.Buffer;
    }
}