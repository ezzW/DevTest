// Emtelaak.UserRegistration.Application/DTOs/AccreditationStatusDto.cs
using System;

namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class AccreditationStatusDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string InvestorClassification { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal? IncomeLevel { get; set; }
        public decimal? NetWorth { get; set; }
        public decimal? InvestmentLimitAmount { get; set; }
        public string ReviewNotes { get; set; }
        public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
        public List<string> RequiredDocuments { get; set; } = new List<string>();
        public string NextStep { get; set; }
    }
}