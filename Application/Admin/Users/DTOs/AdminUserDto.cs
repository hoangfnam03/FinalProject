namespace Application.Admin.Users.DTOs
{
    public class AdminUserDto
    {
        public long MemberId { get; set; }
        public string? Avatar { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Phone { get; set; }
        public string Role { get; set; } = "User"; // User | Moderator | Admin
        public DateTime CreatedAt { get; set; }
    }
}
