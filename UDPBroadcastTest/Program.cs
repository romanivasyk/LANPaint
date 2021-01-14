using LANPaint_vNext.Model;
using LANPaint_vNext.Services;
using LANPaint_vNext.Services.UDP.Test;
using System;
using System.Threading.Tasks;

namespace UDPBroadcastTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string key;
            var _broadcastService = new BroadcastChainer(new UDPBroadcastImpl());
            do
            {
                key = Console.ReadLine();

                var info = new DrawingInfo(ARGBColor.Default, new SerializableStroke(), false, true);
                var serializer = new BinarySerializerService();
                var bytes = serializer.Serialize(info);
                await _broadcastService.SendAsync(bytes);

            } while (key != "q") ;
        }
    }
}
