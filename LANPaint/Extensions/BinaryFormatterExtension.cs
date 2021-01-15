using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LANPaint.Extensions
{
    public static class BinaryFormatterExtension
    {
        public static byte[] OneLineSerialize(this BinaryFormatter formatter, object data)
        {
            byte[] bytes = null;
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, data);
                bytes = stream.ToArray();
            }

            return bytes;
        }

        public static TData OneLineDeserialize<TData>(this BinaryFormatter formatter, byte[] data)
        {
            var deserializedData = default(TData);
            using (var stream = new MemoryStream(data))
            {
                stream.Seek(0, SeekOrigin.Begin);
                deserializedData = (TData)formatter.Deserialize(stream);
            }

            return deserializedData;
        }
    }
}
