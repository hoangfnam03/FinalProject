using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Questions.Commands
{
    public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand>
    {
        private readonly IApplicationDbContext _db;

        public DeletePostCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Handle(DeletePostCommand request, CancellationToken ct)
        {
            var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == request.PostId, ct);
            if (post == null) return;

            // Soft delete: tuỳ base class của Post.
            // Nếu bạn có SoftDeleteInterceptor thì chỉ cần set IsDeleted = true.
            post.IsDeleted = true; // nếu field khác tên -> gửi Post entity
            post.DeletedAt = DateTime.UtcNow; // nếu có

            await _db.SaveChangesAsync(ct);
        }
    }
}
