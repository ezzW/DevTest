// Emtelaak.UserRegistration.Application/DTOs/UpdateAccreditationStatusDto.cs
using System;

namespace Emtelaak.UserRegistration.Application.DTOs
{
    public class UpdateAccreditationStatusDto
    {
        public string Status { get; set; }
        public decimal? InvestmentLimitAmount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string ReviewNotes { get; set; }
    }
}