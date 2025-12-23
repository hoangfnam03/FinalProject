using Application.Common.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Infrastructure.Persistence.Seed
{
    public static class AdminFeaturesSeeder
    {
        public static async Task SeedAsync(IApplicationDbContext db, IIdentityService identity, IFileStorage fileStorage, CancellationToken ct)
        {
            // Idempotent: nếu đã có Posts/Reports/Documents thì skip (tuỳ bạn)
            if (await db.Reports.AnyAsync(ct) || await db.Documents.AnyAsync(ct))
                return;

            // ========== 1) Seed Identity Users + Members ==========
            // Nếu đã có IdentitySeeder rồi thì bạn có thể bỏ phần này,
            // nhưng để test admin thì mình đảm bảo luôn có admin/mod.

            var adminEmail = "admin@test.com";
            var modEmail = "moderator@test.com";

            var adminUserId = await EnsureIdentityUser(identity, adminEmail, "Admin@123");
            var modUserId = await EnsureIdentityUser(identity, modEmail, "Mod@123");

            // tạo Member
            var adminMember = await EnsureMemberAsync(db, adminUserId, "Admin Test", isAdmin: true, isMod: true, ct);
            var modMember = await EnsureMemberAsync(db, modUserId, "Moderator Test", isAdmin: false, isMod: true, ct);

            // tạo thêm 5 users thường
            var userMembers = new List<Member>();
            for (int i = 1; i <= 5; i++)
            {
                var email = $"user{i}@test.com";
                var uid = await EnsureIdentityUser(identity, email, "User@123");
                var m = await EnsureMemberAsync(db, uid, $"User {i}", isAdmin: false, isMod: false, ct);
                userMembers.Add(m);
            }

            // ========== 2) Seed Categories ==========
            // NOTE: Nếu Category entity field name khác (Name/Slug), bạn sửa tại đây.
            var categories = await db.Categories.AsNoTracking().ToListAsync(ct);
            if (categories.Count == 0)
            {
                db.Categories.AddRange(
                    new Category { Name = "Java" },
                    new Category { Name = "React" },
                    new Category { Name = "C#" },
                    new Category { Name = "Database" },
                    new Category { Name = "DevOps" }
                );
                await db.SaveChangesAsync(ct);
                categories = await db.Categories.AsNoTracking().ToListAsync(ct);
            }

            // ========== 3) Seed Posts + Comments (Answered/Unanswered) ==========
            // NOTE: Nếu Post/Comment field name khác, bạn gửi Post.cs + Comment.cs mình sửa đúng 100%.
            var rnd = new Random(123);

            var posts = new List<Post>();

            for (int i = 1; i <= 20; i++)
            {
                var author = userMembers[rnd.Next(userMembers.Count)];
                var cat = categories[rnd.Next(categories.Count)];

                var p = new Post
                {
                    Title = $"[Seed] Question #{i}: How to fix issue {rnd.Next(100, 999)}?",
                    Body = $"This is seeded question content #{i}. Some details here...\n\n- Step A\n- Step B\n\n(Seeded)",
                    AuthorId = author.Id,        // nếu field khác tên => sửa
                    CategoryId = cat.Id        // nếu field khác tên => sửa
                    // CreatedAt thường set bởi AuditingInterceptor; nếu bạn có field bắt buộc thì set thêm ở đây
                };

                posts.Add(p);
            }

            db.Posts.AddRange(posts);
            await db.SaveChangesAsync(ct);

            // tạo comments cho ~1/2 posts để status Answered
            var allPosts = await db.Posts.AsNoTracking().OrderByDescending(x => x.Id).Take(20).ToListAsync(ct);
            var answeredPosts = allPosts.Take(10).ToList();

            var comments = new List<Comment>();
            foreach (var p in answeredPosts)
            {
                var answerAuthor = rnd.Next(0, 3) == 0 ? modMember : adminMember;

                comments.Add(new Comment
                {
                    PostId = p.Id,
                    AuthorId = answerAuthor.Id,
                    Body = $"[Seed Answer] This is an admin/mod answer for post {p.Id}. Try checking configuration and logs."
                });
            }

            db.Comments.AddRange(comments);
            await db.SaveChangesAsync(ct);

            // ========== 4) Seed Reports ==========
            // Tạo vài report pending/resolved để test admin reports page
            var somePost = allPosts.First();
            var someComment = await db.Comments.AsNoTracking().FirstOrDefaultAsync(ct);

            var reports = new List<Report>
            {
                new Report
                {
                    ReporterMemberId = userMembers[0].Id,
                    TargetType = ReportTargetType.Post,
                    TargetId = somePost.Id,
                    Reason = "Spam / quảng cáo",
                    Status = ReportStatus.Pending
                },
                new Report
                {
                    ReporterMemberId = userMembers[1].Id,
                    TargetType = ReportTargetType.Post,
                    TargetId = allPosts.Skip(1).First().Id,
                    Reason = "Nội dung không phù hợp",
                    Status = ReportStatus.Resolved,
                    ResolvedAt = DateTime.UtcNow,
                    ResolvedByMemberId = adminMember.Id
                }
            };

            if (someComment != null)
            {
                reports.Add(new Report
                {
                    ReporterMemberId = userMembers[2].Id,
                    TargetType = ReportTargetType.Comment,
                    TargetId = someComment.Id,
                    Reason = "Comment toxic",
                    Status = ReportStatus.Pending
                });
            }

            db.Reports.AddRange(reports);
            await db.SaveChangesAsync(ct);

            // ========== 5) Seed Documents ==========
            // Tạo file dummy thật qua IFileStorage để test delete (DeleteAsync)
            for (int i = 1; i <= 3; i++)
            {
                var fileName = $"Seed_Document_{i}.docx";
                var bytes = Encoding.UTF8.GetBytes($"Seeded document content #{i} - {DateTime.UtcNow:o}");
                await using var ms = new MemoryStream(bytes);

                var url = await fileStorage.SaveAsync(
                    ms,
                    fileName,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    folder: "admin-documents",
                    ct
                );

                var doc = new Domain.Entities.Document
                {
                    FileName = fileName,
                    StoragePath = url,
                    FileSize = bytes.Length,
                    UploadedByMemberId = adminMember.Id,
                    UploadedAt = DateTime.UtcNow
                };

                db.Documents.Add(doc);
            }

            await db.SaveChangesAsync(ct);
        }

        private static async Task<long> EnsureIdentityUser(IIdentityService identity, string email, string password)
        {
            var existing = await identity.GetUserIdByEmailAsync(email);
            if (existing.HasValue) return existing.Value;

            var userId = await identity.CreateUserAsync(email, password);

            // Auto-confirm email for seeded accounts so dev login works.
            // Keep this only for development seeders; do NOT enable in production.
            var token = await identity.GenerateEmailConfirmationTokenAsync(userId);
            await identity.ConfirmEmailAsync(userId, token);

            return userId;
        }

        private static async Task<Member> EnsureMemberAsync(
            IApplicationDbContext db,
            long userId,
            string displayName,
            bool isAdmin,
            bool isMod,
            CancellationToken ct)
        {
            var existing = await db.Members.FirstOrDefaultAsync(x => x.UserId == userId, ct);
            if (existing != null) return existing;

            var member = new Member
            {
                DisplayName = displayName,
                UserId = userId,
                IsAdministrator = isAdmin,
                IsModerator = isMod,
                TrustLevelId = 1
            };

            db.Members.Add(member);
            await db.SaveChangesAsync(ct);

            return member;
        }
    }
}
