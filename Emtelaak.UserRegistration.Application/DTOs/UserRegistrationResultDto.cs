// Emtelaak.UserRegistration.Application/DTOs/UserRegistrationResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserRegistrationResultDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public bool RequiresEmailVerification { get; set; }
        public bool RequiresPhoneVerification { get; set; }
    }
}
