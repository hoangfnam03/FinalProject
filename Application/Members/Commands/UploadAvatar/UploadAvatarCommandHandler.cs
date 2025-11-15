using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.Commands.UploadAvatar
{
    public class UploadAvatarCommandHandler : IRequestHandler<UploadAvatarCommand, FileDto>
    {
        private static readonly string[] Allowed = new[] { "image/png", "image/jpeg", "image/webp" };
        private const long MaxSize = 5 * 1024 * 1024;

        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly IFileStorage _storage;

        public UploadAvatarCommandHandler(IApplicationDbContext db, ICurrentUserService current, IFileStorage storage)
        { _db = db; _current = current; _storage = storage; }

        public async Task<FileDto> Handle(UploadAvatarCommand request, CancellationToken ct)
        {
            var meId = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            var file = request.File ?? throw new ArgumentException("File is required.");

            if (!Allowed.Contains(file.ContentType)) throw new InvalidOperationException("Unsupported content type.");
            if (file.Length <= 0 || file.Length > MaxSize) throw new InvalidOperationException("Invalid file size.");

            var url = await _storage.SaveAsync(file.OpenReadStream(), file.FileName, file.ContentType, "uploads/avatars", ct);

            var me = await _db.Members.FirstAsync(x => x.Id == meId, ct);
            me.ProfilePictureLink = url; // 👈 đổi sang đúng field

            await _db.SaveChangesAsync(ct);

            return new FileDto { FileName = file.FileName, Url = url, Size = file.Length, ContentType = file.ContentType };
        }
    }
}
