// Emtelaak.UserRegistration.Application/DTOs/UserPreferenceUpdateResultDto.cs
// Emtelaak.UserRegistration.Application/DTOs/UserPreferenceUpdateResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UserPreferenceUpdateResultDto
    {
        public bool Updated { get; set; }
        public UserPreferenceDto Preferences { get; set; }
    }
}
