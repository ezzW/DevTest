// Emtelaak.UserRegistration.Application/DTOs/LoginResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class LoginResultDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public Guid UserId { get; set; }
        public bool RequiresMfa { get; set; }
        public string MfaToken { get; set; }
        public string[] MfaMethods { get; set; }
    }
}
