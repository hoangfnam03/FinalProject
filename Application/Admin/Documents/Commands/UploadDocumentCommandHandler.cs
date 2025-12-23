using Application.Admin.Documents.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Admin.Documents.Commands
{
    public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, DocumentDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly IFileStorage _fileStorage;
        private readonly ICurrentUserService _currentUser;

        public UploadDocumentCommandHandler(IApplicationDbContext db, IFileStorage fileStorage, ICurrentUserService currentUser)
        {
            _db = db;
            _fileStorage = fileStorage;
            _currentUser = currentUser;
        }

        public async Task<DocumentDto> Handle(UploadDocumentCommand request, CancellationToken ct)
        {
            var file = request.File ?? throw new ArgumentNullException(nameof(request.File));
            if (file.Length <= 0) throw new InvalidOperationException("Empty file.");

            var memberId = _currentUser.CurrentMemberId;
            if (memberId == null) throw new UnauthorizedAccessException("Not authenticated.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".doc" && ext != ".docx")
                throw new InvalidOperationException("Only .doc or .docx allowed.");

            // IFileStorage.SaveAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct)
            await using var stream = file.OpenReadStream();

            var savedPath = await _fileStorage.SaveAsync(
                stream,
                fileName: Path.GetFileName(file.FileName),
                contentType: file.ContentType ?? "application/octet-stream",
                folder: "admin-documents",
                ct
            );

            // tránh ambiguous Document: dùng đúng Domain.Entities.Document
            var entity = new Domain.Entities.Document
            {
                FileName = Path.GetFileName(file.FileName),
                StoragePath = savedPath,
                FileSize = file.Length,
                UploadedByMemberId = memberId.Value,
                UploadedAt = DateTime.UtcNow
            };

            _db.Documents.Add(entity);
            await _db.SaveChangesAsync(ct);

            return new DocumentDto
            {
                Id = entity.Id,
                FileName = entity.FileName,
                FileSize = entity.FileSize,
                UploadedAt = entity.UploadedAt
            };
        }
    }
}
