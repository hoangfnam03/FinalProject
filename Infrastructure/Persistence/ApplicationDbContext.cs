using Application.Common.Interfaces;
using Domain.Common;
using Domain.Common.Entities;
using Domain.Entities;
using Domain.Entities.Auth;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, long>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Member> Members => Set<Member>();
        public DbSet<TrustLevel> TrustLevels => Set<TrustLevel>();
        public DbSet<VoteType> VoteTypes => Set<VoteType>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<PostTag> PostTags => Set<PostTag>();
        public DbSet<PostVote> PostVotes => Set<PostVote>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<CommentVote> CommentVotes => Set<CommentVote>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<PostRevision> PostRevisions => Set<PostRevision>();
        public DbSet<CommentRevision> CommentRevisions => Set<CommentRevision>();
        public DbSet<PostAttachment> PostAttachments => Set<PostAttachment>();
        public DbSet<CommentAttachment> CommentAttachments => Set<CommentAttachment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Global filters
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                // Soft delete filter
                if (typeof(SoftDeletableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var method = typeof(ApplicationDbContext)
                        .GetMethod(nameof(ApplySoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                        .MakeGenericMethod(entityType.ClrType);
                    method.Invoke(null, new object[] { builder });
                }
            }
        }

        private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder builder) where TEntity : SoftDeletableEntity
        {
            builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
