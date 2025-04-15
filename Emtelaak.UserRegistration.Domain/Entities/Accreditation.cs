using Emtelaak.UserRegistration.Domain.Enums;

namespace Emtelaak.UserRegistration.Domain.Entities
{
    public class Accreditation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public InvestorClassification InvestorClassification { get; set; }
        public AccreditationStatus Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public decimal? IncomeLevel { get; set; }
        public decimal? NetWorth { get; set; }
        public decimal? InvestmentLimitAmount { get; set; }
        public string ReviewNotes { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        // Navigation property
        public User User { get; set; }
    }
}
