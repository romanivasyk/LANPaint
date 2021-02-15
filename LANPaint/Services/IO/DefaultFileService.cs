using LANPaint.Extensions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace LANPaint.Services.IO
{
    public class DefaultFileService : IFileService
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public async Task SaveToFileAsync(object data, string fileName)
        {
            var bytes = _formatter.OneLineSerialize(data);

            await using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.WriteAsync(bytes);
        }

        public async Task<object> ReadFromFileAsync(string fileName)
        {
            await using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);

            return _formatter.OneLineDeserialize(buffer);
        }
    }
}
