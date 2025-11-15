using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct);
        Task DeleteAsync(string fileUrl, CancellationToken ct);
    }
}
