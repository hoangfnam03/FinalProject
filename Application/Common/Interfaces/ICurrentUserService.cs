namespace Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        long? CurrentMemberId { get; }   // null nếu chưa đăng nhập
        string? CurrentUserId { get; }   // Identity UserId (string) nếu bạn dùng GUID/string
    }
}
