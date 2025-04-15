// Emtelaak.UserRegistration.Application/Interfaces/TokenResult.cs
namespace Emtelaak.UserRegistration.Application.Interfaces
{
    public class TokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
