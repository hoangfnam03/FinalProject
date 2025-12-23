using Domain.Entities;
using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Member> Members { get; }
        DbSet<TrustLevel> TrustLevels { get; }
        DbSet<VoteType> VoteTypes { get; }
        DbSet<RefreshToken> RefreshTokens { get; }
        DbSet<Post> Posts { get; }
        DbSet<Tag> Tags { get; }
        DbSet<PostTag> PostTags { get; }
        DbSet<PostVote> PostVotes { get; }
        DbSet<Comment> Comments { get; }
        DbSet<CommentVote> CommentVotes { get; }
        DbSet<Category> Categories { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<PostRevision> PostRevisions { get; }
        DbSet<CommentRevision> CommentRevisions { get; }
        DbSet<PostAttachment> PostAttachments { get; }
        DbSet<CommentAttachment> CommentAttachments { get; }
        DbSet<Report> Reports { get; }
        DbSet<Document> Documents { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
