// Emtelaak.UserRegistration.Application/DTOs/EmailVerificationResultDto.cs
// Emtelaak.UserRegistration.Application/DTOs/EmailVerificationResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class EmailVerificationResultDto
    {
        public bool Verified { get; set; }
        public string NextStep { get; set; }
        public string Message { get; set; }
        public int? RemainingAttempts { get; set; }
    }
}
