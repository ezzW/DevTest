// Emtelaak.UserRegistration.Application/DTOs/KycVerificationDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class KycVerificationDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string VerificationId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectionReason { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public string RiskLevel { get; set; }
        public string VerificationType { get; set; }
    }
}
