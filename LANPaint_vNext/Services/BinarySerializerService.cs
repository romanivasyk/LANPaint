using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace LANPaint_vNext.Services
{
    public class BinarySerializerService
    {
        private static BinaryFormatter formatter = new BinaryFormatter();

        public byte[] Serialize(object data)
        {
            byte[] bytes = null;

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, data);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        public TData Deserialize<TData>(byte[] data)
        {
            var deserializedData = default(TData);

            using (var stream = new MemoryStream(data))
            {
                stream.Seek(0, SeekOrigin.Begin);
                if (formatter.Deserialize(stream) is TData convertedObject)
                {
                    deserializedData = convertedObject;
                }
            }
            return deserializedData;
        }
    }
}
