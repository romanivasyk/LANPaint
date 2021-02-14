using LANPaint.Extensions;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
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
                try
                {
                    result = await Client.ReceiveAsync().WithCancellation(token);
                }
                catch (OperationCanceledException ex)
                {
                    if (WindowsNative.CancelIoEx(Client.Client.Handle, IntPtr.Zero)) throw;
                    throw new ApplicationException("Unsuccessful attempt to cancel Socket I/O operation", ex);
                }
            } while (Equals(result.RemoteEndPoint, LocalEndPoint));

            return result.Buffer;
        }
    }
}
