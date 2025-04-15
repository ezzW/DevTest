// Emtelaak.UserRegistration.Application/DTOs/PasswordChangeDto.cs
// Emtelaak.UserRegistration.Application/DTOs/PasswordChangeDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class PasswordChangeDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
