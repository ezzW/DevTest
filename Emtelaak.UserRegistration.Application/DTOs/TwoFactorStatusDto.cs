// Emtelaak.UserRegistration.Application/DTOs/TwoFactorStatusDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class TwoFactorStatusDto
    {
        public bool Enabled { get; set; }
        public string Method { get; set; }
    }
}
