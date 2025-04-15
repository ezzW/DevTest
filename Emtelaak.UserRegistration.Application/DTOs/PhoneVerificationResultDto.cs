// Emtelaak.UserRegistration.Application/DTOs/PhoneVerificationResultDto.cs
// Emtelaak.UserRegistration.Application/DTOs/PhoneVerificationResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class PhoneVerificationResultDto
    {
        public bool Verified { get; set; }
        public string NextStep { get; set; }
        public string Message { get; set; }
        public int? RemainingAttempts { get; set; }
    }
}
