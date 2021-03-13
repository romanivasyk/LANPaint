using System.Threading.Tasks;

namespace LANPaint.Services.IO
{
    public interface IFileService
    {
        public Task SaveToFileAsync(object info, string fileName);
        public Task<object> ReadFromFileAsync(string fileName);
    }
}
