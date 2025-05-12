// Emtelaak.UserRegistration.Application/DTOs/UpdateAccreditationStatusResultDto.cs
using System;

namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UpdateAccreditationStatusResultDto
    {
        public Guid AccreditationId { get; set; }
        public string Status { get; set; }
        public string InvestorClassification { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public decimal? InvestmentLimitAmount { get; set; }
        public string ReviewNotes { get; set; }
        public bool Successful { get; set; }
        public string Message { get; set; }
    }
}