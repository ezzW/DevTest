// Emtelaak.UserRegistration.Application/DTOs/KycSubmissionResultDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class KycSubmissionResultDto
    {
        public string VerificationId { get; set; }
        public string Status { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}