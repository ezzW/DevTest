// Emtelaak.UserRegistration.Application/DTOs/AccreditationDto.cs
namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class AccreditationDto
    {
        public Guid Id { get; set; }
        public string InvestorClassification { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal? IncomeLevel { get; set; }
        public decimal? NetWorth { get; set; }
        public decimal? InvestmentLimitAmount { get; set; }
        public string ReviewNotes { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
