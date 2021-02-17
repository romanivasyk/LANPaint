using System;
using System.Net;
using System.Threading.Tasks;

namespace LANPaint.Services.Broadcast
{
    public interface IBroadcastService : IDisposable
    {
        event Action ConnectionLost;
        event DataReceivedEventHandler DataReceived;

        public bool IsReady { get; }
        public IPEndPoint LocalEndPoint { get; }

        public bool Initialize(IPAddress ipAddress, int port = default);
        public Task<int> SendAsync(byte[] data);
        public Task StartReceiveAsync();
        public void CancelReceive();
    }

    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);
    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }

        public DataReceivedEventArgs(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data), "Data cannot be null.");
            if (data.Length < 1) throw new ArgumentException("Data array could not be empty.", nameof(data));

            Data = data;
        }
    }
}
