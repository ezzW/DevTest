// Emtelaak.UserRegistration.Application/DTOs/LoginDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}

