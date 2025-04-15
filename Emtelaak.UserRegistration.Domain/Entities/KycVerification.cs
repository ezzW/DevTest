// Emtelaak.UserRegistration.Domain/Entities/User.cs

using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class KycVerification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public KycStatus Status { get; set; }
        public string VerificationId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string RejectionReason { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public VerificationType VerificationType { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}