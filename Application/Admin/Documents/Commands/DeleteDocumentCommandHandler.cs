using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Documents.Commands
{
    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IFileStorage _fileStorage;

        public DeleteDocumentCommandHandler(IApplicationDbContext db, IFileStorage fileStorage)
        {
            _db = db;
            _fileStorage = fileStorage;
        }

        public async Task Handle(DeleteDocumentCommand request, CancellationToken ct)
        {
            var doc = await _db.Documents.FirstOrDefaultAsync(x => x.Id == request.DocumentId, ct);
            if (doc == null) return;

            // delete physical file
            await _fileStorage.DeleteAsync(doc.StoragePath, ct);

            // soft delete
            doc.IsDeleted = true;
            doc.DeletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}
