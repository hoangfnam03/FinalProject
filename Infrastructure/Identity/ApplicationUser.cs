using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity
{
    // Khóa kiểu long để khớp với Member.UserId (long)
    public class ApplicationUser : IdentityUser<long>
    {
        // Không thêm field profile ở đây; mọi hồ sơ để ở Member (Domain)
    }
}
