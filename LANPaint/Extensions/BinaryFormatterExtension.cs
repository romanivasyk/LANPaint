using Microsoft.IO;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LANPaint.Extensions
{
    public static class BinaryFormatterExtension
    {
        private static readonly RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();

        public static byte[] OneLineSerialize(this BinaryFormatter formatter, object data)
        {
            using var stream = MemoryStreamManager.GetStream();
            formatter.Serialize(stream, data);
            var bytes = stream.ToArray();

            return bytes;
        }

        public static TData OneLineDeserialize<TData>(this BinaryFormatter formatter, byte[] bytes)
        {
            using var stream = MemoryStreamManager.GetStream();
            stream.Write(bytes, 0, bytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            var data = (TData)formatter.Deserialize(stream);

            return data;
        }
    }
}
