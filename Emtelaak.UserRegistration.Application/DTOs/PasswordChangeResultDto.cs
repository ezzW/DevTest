// Emtelaak.UserRegistration.Application/DTOs/PasswordChangeResultDto.cs
// Emtelaak.UserRegistration.Application/DTOs/PasswordChangeResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class PasswordChangeResultDto
    {
        public bool Updated { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}
