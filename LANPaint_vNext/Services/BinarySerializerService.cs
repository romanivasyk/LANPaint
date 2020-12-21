using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace LANPaint_vNext.Services
{
    public class BinarySerializerService
    {
        private static readonly object _locker = new object();
        private static BinaryFormatter formatter = new BinaryFormatter();

        public byte[] Serialize(object data)
        {
            byte[] bytes = null;

            using (var stream = new MemoryStream())
            {
                lock (_locker)
                {
                    formatter.Serialize(stream, data);
                }
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

                lock (_locker)
                {
                    if (formatter.Deserialize(stream) is TData convertedObject)
                    {
                        deserializedData = convertedObject;
                    }
                }
            }
            return deserializedData;
        }
    }
}
