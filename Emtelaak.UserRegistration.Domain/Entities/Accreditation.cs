using Emtelaak.UserRegistration.Domain.Enums;
using System.Collections.Generic;

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
        
        // Financial Information
        public decimal? IncomeLevel { get; set; }
        public decimal? NetWorth { get; set; }
        public decimal? InvestmentLimitAmount { get; set; }
        
        // Investment Experience
        public int? YearsInvesting { get; set; }
        public bool HasPriorPrivateInvestments { get; set; }
        public string InvestmentExperience { get; set; } // JSON string for investment types/experience
        
        // Entity Information (for Institutional Investors)
        public string EntityName { get; set; }
        public string EntityType { get; set; } // LLC, Corporation, Partnership, etc.
        public string EntityRegistrationNumber { get; set; }
        public DateTime? EntityRegistrationDate { get; set; }
        
        // Accreditation Details
        public string CertificationMethod { get; set; } // Self-certification, Third-party verification
        public string VerificationProviderId { get; set; } // Third-party verification reference
        public DateTime? VerificationDate { get; set; }
        
        // Administrative
        public string ReviewNotes { get; set; }
        public string RejectionReason { get; set; }
        public bool OverrideEnabled { get; set; }
        public Guid? OverrideBy { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
