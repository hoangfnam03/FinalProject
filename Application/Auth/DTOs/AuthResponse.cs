namespace Application.Auth.DTOs
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = default!;
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; } = default!;
    }
}
