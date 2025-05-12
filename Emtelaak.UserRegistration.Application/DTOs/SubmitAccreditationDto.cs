// Emtelaak.UserRegistration.Application/DTOs/SubmitAccreditationDto.cs
using System;
using System.Collections.Generic;

namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class SubmitAccreditationDto
    {
        // Basic classification
        public string InvestorClassification { get; set; }
        
        // Financial information
        public decimal? IncomeLevel { get; set; }
        public decimal? NetWorth { get; set; }
        
        // Investment experience
        public int? YearsInvesting { get; set; }
        public bool HasPriorPrivateInvestments { get; set; }
        public List<string> InvestmentTypes { get; set; } = new List<string>();
        
        // Entity information (for institutional investors)
        public string EntityName { get; set; }
        public string EntityType { get; set; }
        public string EntityRegistrationNumber { get; set; }
        public DateTime? EntityRegistrationDate { get; set; }
        
        // Certification information
        public string CertificationMethod { get; set; }
        public List<Guid> DocumentIds { get; set; } = new List<Guid>();
        
        // Additional information
        public string AdditionalInformation { get; set; }
    }

    public class InvestmentExperienceDto
    {
        public string InvestmentType { get; set; }  // e.g., Stocks, Bonds, Real Estate, Private Equity
        public int YearsExperience { get; set; }
        public decimal ApproximateAmount { get; set; }
    }
}