// Emtelaak.UserRegistration.Application/DTOs/ResetPasswordResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class ResetPasswordResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }
    }
}