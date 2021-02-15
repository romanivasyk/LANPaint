using System.Threading.Tasks;

namespace LANPaint.Services.IO
{
    public interface IFileService
    {
        Task SaveToFileAsync(object info, string fileName);
        Task<object> ReadFromFileAsync(string fileName);
    }
}
