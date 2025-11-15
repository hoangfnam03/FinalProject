using Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;

        public LocalFileStorage(IWebHostEnvironment env) => _env = env;

        public async Task<string> SaveAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var dir = Path.Combine(webRoot, folder);
            Directory.CreateDirectory(dir);

            var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
            var fullPath = Path.Combine(dir, safeName);

            using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await stream.CopyToAsync(fs, ct);
            }

            var url = $"/{folder.Replace("\\", "/")}/{safeName}";
            return url;
        }

        public Task DeleteAsync(string fileUrl, CancellationToken ct)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var localPath = Path.Combine(webRoot, fileUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(localPath)) File.Delete(localPath);
            return Task.CompletedTask;
        }
    }
}
