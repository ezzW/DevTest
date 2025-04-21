// Emtelaak.UserRegistration.Application/DTOs/ResetPasswordDto.cs
// Emtelaak.UserRegistration.Application/DTOs/ResetPasswordDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class ResetPasswordDto
    {
        public string Email { get; set; }
        public string ResetCode { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

    }
}
