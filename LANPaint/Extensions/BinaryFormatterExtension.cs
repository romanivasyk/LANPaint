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
            byte[] bytes = null;

            using var stream = MemoryStreamManager.GetStream();
            formatter.Serialize(stream, data);
            bytes = stream.ToArray();

            return bytes;
        }

        public static TData OneLineDeserialize<TData>(this BinaryFormatter formatter, byte[] data)
        {
            var deserializedData = default(TData);

            using var stream = MemoryStreamManager.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Seek(0, SeekOrigin.Begin);
            deserializedData = (TData)formatter.Deserialize(stream);

            return deserializedData;
        }
    }
}
