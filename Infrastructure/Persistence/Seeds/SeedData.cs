using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Seeds
{
    public static class SeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!await context.TrustLevels.AnyAsync())
            {
                await context.TrustLevels.AddRangeAsync(
                    new TrustLevel { Name = "Newbie" },
                    new TrustLevel { Name = "Contributor" },
                    new TrustLevel { Name = "Curator" }
                );
            }

            if (!await context.VoteTypes.AnyAsync())
            {
                await context.VoteTypes.AddRangeAsync(
                    new VoteType { Name = "Upvote" },
                    new VoteType { Name = "Downvote" },
                    new VoteType { Name = "Accept" }
                );
            }

            await context.SaveChangesAsync();
        }
    }
}
