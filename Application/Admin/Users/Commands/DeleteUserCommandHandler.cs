using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Users.Commands
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
    {
        private readonly IApplicationDbContext _db;
        private readonly IIdentityService _identity;

        public DeleteUserCommandHandler(IApplicationDbContext db, IIdentityService identity)
        {
            _db = db;
            _identity = identity;
        }

        public async Task Handle(DeleteUserCommand request, CancellationToken ct)
        {
            var member = await _db.Members.FirstOrDefaultAsync(x => x.Id == request.MemberId, ct);
            if (member == null) return;

            // soft delete member
            member.IsDeleted = true;
            member.DeletedAt = DateTime.UtcNow;

            // lock/delete identity user
            await _identity.LockUserAsync(member.UserId, ct); // NEED YOU SEND interface

            await _db.SaveChangesAsync(ct);
        }
    }
}
