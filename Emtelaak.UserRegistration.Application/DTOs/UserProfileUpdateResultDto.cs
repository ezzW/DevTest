// Emtelaak.UserRegistration.Application/DTOs/UserProfileUpdateResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserProfileUpdateResultDto
    {
        public Guid UserId { get; set; }
        public bool Updated { get; set; }
        public UserProfileUpdateDto Profile { get; set; }
    }
}
