using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json; // Cần cái này để seed DataJson
using System.Threading.Tasks;
using Domain.Common.Enums; // Đảm bảo bạn đã có Enum này
using Domain.Entities;
using Infrastructure.Identity; // Namespace chứa ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seed
{
    public static class QnASeeder
    {
        private static readonly Random _random = new();

        public static async Task SeedAsync(
            ApplicationDbContext ctx,
            UserManager<ApplicationUser> userManager)
        {
            // 1. TrustLevels
            await EnsureTrustLevelsAsync(ctx);

            // 2. Users & Members
            await EnsureDemoMembersAsync(ctx, userManager);

            // 3. Reset Data cũ (Cẩn thận khi dùng trên production)
            await ResetQnADataAsync(ctx);

            // 4. Categories & Tags (Y tế)
            await SeedMedicalCategoriesAndTagsAsync(ctx);

            // 5. Posts & Comments (Nội dung Y tế)
            await SeedMedicalContentAsync(ctx);

            // 6. Votes & Revisions & Notifications (Tương tác)
            await SeedInteractionDataAsync(ctx);
        }

        // ==========================================================
        // 1. TRUST LEVELS
        // ==========================================================
        private static async Task EnsureTrustLevelsAsync(ApplicationDbContext ctx)
        {
            if (await ctx.TrustLevels.AnyAsync()) return;

            var levels = new List<TrustLevel>
            {
                new() { Name = "Newbie", Description = "Thành viên mới" },
                new() { Name = "Member", Description = "Thành viên chính thức" },
                new() { Name = "Trusted", Description = "Thành viên uy tín" },
                new() { Name = "Expert", Description = "Chuyên gia y tế / Bác sĩ" },
                new() { Name = "Admin", Description = "Quản trị viên" }
            };
            await ctx.TrustLevels.AddRangeAsync(levels);
            await ctx.SaveChangesAsync();
        }

        // ==========================================================
        // 2. USERS & MEMBERS
        // ==========================================================
        private static async Task EnsureDemoMembersAsync(
            ApplicationDbContext ctx,
            UserManager<ApplicationUser> userManager)
        {
            if (await ctx.Members.AnyAsync()) return;

            var trustLevels = await ctx.TrustLevels.ToListAsync();
            var newbieLevel = trustLevels.First(t => t.Name == "Newbie").Id;
            var expertLevel = trustLevels.FirstOrDefault(t => t.Name == "Expert")?.Id ?? newbieLevel;

            // Danh sách user demo
            var demoUsers = new[]
            {
                new { Email = "admin@hospital.com", Name = "Admin Quản Trị", IsAdmin = true, IsMod = false, LevelId = expertLevel },
                new { Email = "bs_hung@hospital.com", Name = "BS. CK1 Nguyễn Văn Hùng", IsAdmin = false, IsMod = true, LevelId = expertLevel },
                new { Email = "bs_lan@hospital.com", Name = "BS. Trần Thị Lan", IsAdmin = false, IsMod = true, LevelId = expertLevel },
                new { Email = "benhnhan_an@gmail.com", Name = "Nguyễn Văn An", IsAdmin = false, IsMod = false, LevelId = newbieLevel },
                new { Email = "benhnhan_bich@gmail.com", Name = "Lê Thị Bích", IsAdmin = false, IsMod = false, LevelId = newbieLevel },
                new { Email = "benhnhan_tu@gmail.com", Name = "Hoàng Tuấn Tú", IsAdmin = false, IsMod = false, LevelId = newbieLevel },
            };

            foreach (var du in demoUsers)
            {
                var user = await userManager.FindByEmailAsync(du.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = du.Email,
                        Email = du.Email,
                        EmailConfirmed = true,
                        // Nếu class ApplicationUser của bạn có trường AvatarUrl thì gán ở đây
                    };
                    await userManager.CreateAsync(user, "Pa$$w0rd");
                }

                if (!await ctx.Members.AnyAsync(m => m.UserId == user.Id))
                {
                    ctx.Members.Add(new Member
                    {
                        UserId = user.Id,
                        DisplayName = du.Name,
                        Bio = du.IsMod ? "Chuyên gia tư vấn sức khỏe." : "Thành viên diễn đàn hỏi đáp y tế.",
                        IsAdministrator = du.IsAdmin,
                        IsModerator = du.IsMod,
                        TrustLevelId = du.LevelId,
                        ProfilePictureLink = $"https://ui-avatars.com/api/?name={du.Name}&background=random&size=128"
                    });
                }
            }
            await ctx.SaveChangesAsync();
        }

        // ==========================================================
        // 3. SEED CATEGORIES & TAGS
        // ==========================================================
        private static async Task SeedMedicalCategoriesAndTagsAsync(ApplicationDbContext ctx)
        {
            // Categories
            if (!await ctx.Categories.AnyAsync())
            {
                var cats = new List<Category>
                {
                    new() { Name = "Tiêm chủng", Slug = "tiem-chung", DisplayOrder = 1, IsHidden = false },
                    new() { Name = "Nội khoa", Slug = "noi-khoa", DisplayOrder = 2, IsHidden = false },
                    new() { Name = "Nhi khoa", Slug = "nhi-khoa", DisplayOrder = 3, IsHidden = false },
                    new() { Name = "Sản phụ khoa", Slug = "san-phu-khoa", DisplayOrder = 4, IsHidden = false },
                    new() { Name = "Dinh dưỡng", Slug = "dinh-duong", DisplayOrder = 5, IsHidden = false },
                    new() { Name = "Covid-19", Slug = "covid-19", DisplayOrder = 6, IsHidden = false },
                };
                await ctx.Categories.AddRangeAsync(cats);
                await ctx.SaveChangesAsync();
            }

            // Tags
            if (!await ctx.Tags.AnyAsync())
            {
                var tags = new List<Tag>
                {
                    new() { Name = "viem-gan-b", Slug = "viem-gan-b" },
                    new() { Name = "vaccine", Slug = "vaccine" },
                    new() { Name = "sot-xuat-huyet", Slug = "sot-xuat-huyet" },
                    new() { Name = "tre-em", Slug = "tre-em" },
                    new() { Name = "huyet-ap", Slug = "huyet-ap" },
                    new() { Name = "tieu-duong", Slug = "tieu-duong" },
                    new() { Name = "xuong-khop", Slug = "xuong-khop" },
                    new() { Name = "mang-thai", Slug = "mang-thai" }
                };
                await ctx.Tags.AddRangeAsync(tags);
                await ctx.SaveChangesAsync();
            }
        }

        // ==========================================================
        // 4. SEED POSTS & COMMENTS
        // ==========================================================
        private static async Task SeedMedicalContentAsync(ApplicationDbContext ctx)
        {
            if (await ctx.Posts.AnyAsync()) return;

            var members = await ctx.Members.ToListAsync();
            var categories = await ctx.Categories.ToListAsync();
            var tags = await ctx.Tags.ToListAsync();

            var patients = members.Where(m => !m.IsModerator && !m.IsAdministrator).ToList();
            var experts = members.Where(m => m.IsModerator).ToList();

            if (!patients.Any()) return;

            var posts = new List<Post>();

            // --- DATA MẪU CỐ ĐỊNH (Theo yêu cầu của bạn) ---
            var sampleQnA = new[] {
                new {
                    Title = "Đang chích ngừa viêm gan B có chích ngừa Covid-19 được không?",
                    Body = "Tôi đang trong liệu trình tiêm viêm gan B, tuần sau là lịch tiêm mũi 2 nhưng công ty lại tổ chức tiêm Covid. Liệu tôi có tiêm được không?",
                    Answer = "Nếu anh/chị đang tiêm ngừa vaccine phòng bệnh viêm gan B, anh/chị vẫn có thể tiêm phòng vaccine phòng Covid-19, tuy nhiên vaccine Covid-19 phải được tiêm cách trước và sau mũi vaccine viêm gan B tối thiểu là 14 ngày.",
                    Cat = "Tiêm chủng",
                    TagNames = new[]{"viem-gan-b", "vaccine", "covid-19"}
                },
                new {
                    Title = "Trẻ bị sốt xuất huyết có được tắm không?",
                    Body = "Con tôi 5 tuổi bị sốt xuất huyết ngày thứ 3, người ra nhiều mồ hôi. Tôi có nên tắm cho cháu không?",
                    Answer = "Chào bạn, trẻ bị sốt xuất huyết VẪN CÓ THỂ tắm rửa hoặc lau người bằng nước ấm trong phòng kín gió. Tuyệt đối không dùng nước lạnh vì sẽ làm co mạch ngoài da nhưng lại làm giãn mạch nội tạng rất nguy hiểm.",
                    Cat = "Nhi khoa",
                    TagNames = new[]{"sot-xuat-huyet", "tre-em"}
                },
                new {
                    Title = "Uống thuốc huyết áp vào thời điểm nào là tốt nhất?",
                    Body = "Bác sĩ kê đơn thuốc huyết áp uống hàng ngày nhưng tôi hay quên. Nên uống sáng hay tối?",
                    Answer = "Theo khuyến cáo chung, nên uống thuốc huyết áp vào buổi sáng để kiểm soát sự gia tăng huyết áp sinh lý trong ngày.",
                    Cat = "Nội khoa",
                    TagNames = new[]{"huyet-ap"}
                }
            };

            foreach (var item in sampleQnA)
            {
                var author = RandomPick(patients);
                var category = categories.FirstOrDefault(c => c.Name == item.Cat) ?? categories.First();

                var post = new Post
                {
                    Title = item.Title,
                    Body = item.Body,
                    AuthorId = author.Id,
                    CategoryId = category.Id,
                    // Nếu AuditableEntity có CreatedDate thì gán ở đây, ví dụ:
                    // CreatedDate = DateTime.Now.AddDays(-5)
                };
                posts.Add(post);
            }

            // --- DATA RANDOM CHO ĐỦ SỐ LƯỢNG ---
            for (int i = 0; i < 20; i++)
            {
                var cat = RandomPick(categories);
                posts.Add(new Post
                {
                    Title = $"Câu hỏi về {cat.Name} số {i + 1}: Triệu chứng kéo dài?",
                    Body = $"Chào bác sĩ, dạo gần đây tôi thấy mệt mỏi và có các dấu hiệu lạ... Mong bác sĩ tư vấn giúp.",
                    AuthorId = RandomPick(patients).Id,
                    CategoryId = cat.Id,
                });
            }

            await ctx.Posts.AddRangeAsync(posts);
            await ctx.SaveChangesAsync(); // Save để lấy PostId

            // --- GÁN TAGS & TẠO COMMENTS ---
            var postTags = new List<PostTag>();
            var comments = new List<Comment>();

            // Lấy lại posts đã có ID
            var savedPosts = await ctx.Posts.ToListAsync();

            int qnaIndex = 0;
            foreach (var p in savedPosts)
            {
                // 1. Gán Tags
                // Nếu là bài mẫu cố định
                if (qnaIndex < sampleQnA.Length)
                {
                    var item = sampleQnA[qnaIndex];
                    foreach (var tagName in item.TagNames)
                    {
                        var t = tags.FirstOrDefault(x => x.Name == tagName);
                        if (t != null) postTags.Add(new PostTag { PostId = p.Id, TagId = t.Id });
                    }

                    // 2. Tạo câu trả lời mẫu của bác sĩ
                    if (experts.Any())
                    {
                        comments.Add(new Comment
                        {
                            PostId = p.Id,
                            AuthorId = RandomPick(experts).Id,
                            Body = item.Answer
                        });
                    }
                    qnaIndex++;
                }
                else
                {
                    // Bài random
                    var t = RandomPick(tags);
                    postTags.Add(new PostTag { PostId = p.Id, TagId = t.Id });

                    // Random comment (đôi khi không có trả lời)
                    if (_random.NextDouble() > 0.3)
                    {
                        comments.Add(new Comment
                        {
                            PostId = p.Id,
                            AuthorId = experts.Any() ? RandomPick(experts).Id : RandomPick(patients).Id,
                            Body = "Trường hợp này bạn nên đi khám trực tiếp nhé."
                        });
                    }
                }
            }

            await ctx.PostTags.AddRangeAsync(postTags);
            await ctx.Comments.AddRangeAsync(comments);
            await ctx.SaveChangesAsync();
        }

        // ==========================================================
        // 5. SEED INTERACTION (VOTES, REVISIONS, NOTIFICATIONS)
        // ==========================================================
        private static async Task SeedInteractionDataAsync(ApplicationDbContext ctx)
        {
            var members = await ctx.Members.ToListAsync();
            var posts = await ctx.Posts.ToListAsync();
            var comments = await ctx.Comments.ToListAsync();

            // --- 5.1 PostVotes ---
            // Entity PostVote: PostId, MemberId, Value (+1/-1)
            var postVotes = new List<PostVote>();
            foreach (var p in posts)
            {
                int votes = _random.Next(0, 5);
                var voters = members.Where(m => m.Id != p.AuthorId).OrderBy(x => Guid.NewGuid()).Take(votes);
                foreach (var v in voters)
                {
                    postVotes.Add(new PostVote
                    {
                        PostId = p.Id,
                        MemberId = v.Id,
                        Value = 1 // Upvote
                    });
                }
            }
            if (!await ctx.PostVotes.AnyAsync()) await ctx.PostVotes.AddRangeAsync(postVotes);

            // --- 5.2 CommentVotes ---
            // Entity CommentVote: CommentId, MemberId, Value
            var commentVotes = new List<CommentVote>();
            foreach (var c in comments)
            {
                int votes = _random.Next(0, 3);
                var voters = members.Where(m => m.Id != c.AuthorId).OrderBy(x => Guid.NewGuid()).Take(votes);
                foreach (var v in voters)
                {
                    commentVotes.Add(new CommentVote
                    {
                        CommentId = c.Id,
                        MemberId = v.Id,
                        Value = 1
                    });
                }
            }
            if (!await ctx.CommentVotes.AnyAsync()) await ctx.CommentVotes.AddRangeAsync(commentVotes);

            // --- 5.3 PostRevisions (Lịch sử sửa bài) ---
            // Entity PostRevision: PostId, BeforeTitle, AfterTitle, BeforeBody, AfterBody, EditorId
            var postRevisions = new List<PostRevision>();
            var editedPosts = posts.Take(3); // Giả sử 3 bài đầu bị sửa
            foreach (var p in editedPosts)
            {
                postRevisions.Add(new PostRevision
                {
                    PostId = p.Id,
                    EditorId = p.AuthorId, // Tự sửa
                    BeforeTitle = p.Title,
                    AfterTitle = p.Title + " [Đã bổ sung]",
                    BeforeBody = "Nội dung cũ chưa đầy đủ...",
                    AfterBody = p.Body,
                    Summary = "Cập nhật thêm thông tin bệnh án"
                });
            }
            if (!await ctx.PostRevisions.AnyAsync()) await ctx.PostRevisions.AddRangeAsync(postRevisions);

            // --- 5.4 Notifications ---
            // Entity Notification: RecipientId, ActorId, Type (enum), PostId, CommentId, DataJson
            // Lưu ý: Entity của bạn dùng 'DataJson', không có 'Message' trực tiếp.
            var notifications = new List<Notification>();
            foreach (var c in comments)
            {
                var post = posts.FirstOrDefault(p => p.Id == c.PostId);
                if (post != null && post.AuthorId != c.AuthorId)
                {
                    // Tạo message JSON
                    var notiData = new { message = $"đã trả lời câu hỏi: {post.Title}" };

                    notifications.Add(new Notification
                    {
                        RecipientId = post.AuthorId, // Gửi cho chủ bài viết
                        ActorId = c.AuthorId,        // Người comment
                        Type = NotificationType.PostCommented, // Cần đảm bảo Enum này tồn tại
                        PostId = post.Id,
                        CommentId = c.Id,
                        IsRead = _random.NextDouble() > 0.5,
                        DataJson = JsonSerializer.Serialize(notiData) // Serialize message vào JSON
                    });
                }
            }
            if (!await ctx.Notifications.AnyAsync()) await ctx.Notifications.AddRangeAsync(notifications);

            await ctx.SaveChangesAsync();
        }

        // =============================
        // HELPERS
        // =============================
        private static async Task ResetQnADataAsync(ApplicationDbContext ctx)
        {
            // Xóa theo thứ tự để tránh lỗi khóa ngoại (Foreign Key)
            ctx.Notifications.RemoveRange(await ctx.Notifications.ToListAsync());
            ctx.PostVotes.RemoveRange(await ctx.PostVotes.ToListAsync());
            ctx.CommentVotes.RemoveRange(await ctx.CommentVotes.ToListAsync());
            ctx.PostRevisions.RemoveRange(await ctx.PostRevisions.ToListAsync());
            ctx.CommentRevisions.RemoveRange(await ctx.CommentRevisions.ToListAsync());
            ctx.PostTags.RemoveRange(await ctx.PostTags.ToListAsync());
            ctx.Comments.RemoveRange(await ctx.Comments.ToListAsync());
            ctx.Posts.RemoveRange(await ctx.Posts.ToListAsync());
            ctx.Tags.RemoveRange(await ctx.Tags.ToListAsync());
            ctx.Categories.RemoveRange(await ctx.Categories.ToListAsync());

            await ctx.SaveChangesAsync();
        }

        private static T RandomPick<T>(IList<T> list)
        {
            return list[_random.Next(list.Count)];
        }
    }
}