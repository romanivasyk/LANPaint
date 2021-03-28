using System;
using LANPaint.Extensions;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace LANPaint.Services.IO
{
    public class DefaultFileService : IFileService
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private readonly string[] _allowedExtensions;

        public DefaultFileService(string[] allowedExtensions)
        {
            if (allowedExtensions == null) throw new ArgumentNullException(nameof(allowedExtensions));
            if (allowedExtensions.All(ext => ext == null))
                throw new ArgumentException($"\"{allowedExtensions}\" cannot contains only nulls!");

            _allowedExtensions = allowedExtensions;
        }

        public async Task SaveToFileAsync(object data, string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (!Path.HasExtension(fileName) || !_allowedExtensions.Contains(Path.GetExtension(fileName)))
                throw new ArgumentException($"\"{fileName}\" doesn't contain valid extension.");

            var bytes = _formatter.OneLineSerialize(data);

            await using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.WriteAsync(bytes);
        }

        public async Task<object> ReadFromFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (!File.Exists(fileName) || !Path.HasExtension(fileName) ||
                _allowedExtensions.All(ext => ext != Path.GetExtension(fileName)))
                throw new ArgumentException($"\"{fileName}\" doesn't contain valid extension.");

            await using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);

            return _formatter.OneLineDeserialize(buffer);
        }
    }
}