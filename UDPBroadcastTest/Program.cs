using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPBroadcastTest
{
    class Program
    {
        static UdpClient udpClient = new UdpClient();
        static void Main(string[] args)
        {
            int PORT = 9876;
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
            Receive();
            Task.Delay(100).Wait();

            var input = string.Empty;
            while (true)
            {
                Console.Write("Send->");
                input = Console.ReadLine();
                var data = Encoding.UTF8.GetBytes(input);
                udpClient.Send(data, data.Length, "255.255.255.255", PORT);
                Task.Delay(100).Wait();
            }
        }

        static void Receive()
        {
            var from = new IPEndPoint(0, 0);
            Task.Run(() =>
            {
                while (true)
                {
                    var recvBuffer = udpClient.Receive(ref from);
                    Console.WriteLine($"Received: {Encoding.UTF8.GetString(recvBuffer)}");
                }
            });

        }
    }
}
